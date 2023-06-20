using System.Buffers;
using System.Text.Json;
using Microsoft.AspNetCore.Connections;
using Serilog;
using XGFramework.Services;

namespace XGFramework;

public class ServerConnectionHandler : ConnectionHandler, IRpcReply
{
    private readonly IActorService  _actorService;
    private readonly ISenderManager _senderManager;

    public ServerConnectionHandler(IActorService actorService, ISenderManager senderManager)
    {
        _actorService  = actorService;
        _senderManager = senderManager;
    }

    public override async Task OnConnectedAsync(ConnectionContext connection)
    {
        try
        {
            await HandlerAsync(connection);
        }
        catch (TaskCanceledException)
        {
            // 忽略
        }
        catch (Exception e)
        {
            Log.Error(e, "");
        }
        finally
        {
            await connection.DisposeAsync();
        }
    }

    private async Task HandlerAsync(ConnectionContext context)
    {
        var input = context.Transport.Input;

        while (context.ConnectionClosed.IsCancellationRequested == false)
        {
            var result = await input.ReadAsync(context.ConnectionClosed);
            if (result.IsCanceled || result.IsCompleted)
                break;

            var buffer = result.Buffer;
            try
            {
                while (TryParse(ref buffer, out ActorMessage message))
                {
                    if (message.Type is MessageType.Response)
                    {
                        if (_senderManager.TryGet(message.ActorId.PId, out var sender))
                        {
                            sender.OnResponse(message);
                        }
                    }
                    else
                    {
                        if (message.Type is MessageType.Request)
                            message.RpcReply = this;
                        ActorMailbox mailbox = _actorService.GetMailbox(message.ActorId);
                        mailbox.Enqueue(message);
                    }
                }
            }
            finally
            {
                // 第一个参数确定消耗的内存量。
                // 第二个参数确定观察到的缓冲区数。
                // https://learn.microsoft.com/zh-cn/dotnet/standard/io/pipelines#pipereader
                input.AdvanceTo(buffer.Start, buffer.End);
            }
        }
    }

    /// <summary>
    /// 内部网络消息头: length[4] + actorId[8] + type[1] + opcode[2] + rpcId[4] + body ...
    /// length都要算,但不包括自己.
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private static bool TryParse(ref ReadOnlySequence<byte> buffer, out ActorMessage message)
    {
        message = default;
        if (buffer.Length < MessageHeaderHelper.MinSize)
            return false;

        SequenceReader<byte> reader = new SequenceReader<byte>(buffer);
        if (!reader.TryReadBigEndian(out int length))
            return false;

        if (buffer.Length < length + MessageHeaderHelper.LengthFieldSize)
            return false;

        // 更新buffer，以便从缓冲区中裁剪已读取的消息。
        buffer = buffer.Slice(length + MessageHeaderHelper.LengthFieldSize);

        reader.TryReadBigEndian(out ulong actorId);
        reader.TryRead(out byte type);
        reader.TryReadBigEndian(out ushort opcode);
        uint rpcId = 0;
        if (type is MessageType.Request or MessageType.Response)
        {
            reader.TryReadBigEndian(out uint tmpRpcId);
            rpcId = tmpRpcId;
        }

        // 先用json
        if (!OpcodeCollection.Ins.TryGetType(opcode, out Type t))
        {
            Console.WriteLine($"找不到opcode对应Type: opcode={opcode}");
            return false;
        }

        // 先用json代替。
        Utf8JsonReader jsonReader = new Utf8JsonReader(reader.UnreadSequence);
        var            body       = JsonSerializer.Deserialize(ref jsonReader, t);
        message = new ActorMessage(type, actorId, opcode, (IMessageBase)body, rpcId);
        return true;
    }

    void IRpcReply.Invoke(IResponse resp, ActorMessage context)
    {
        if (_senderManager.TryGet(context.ActorId.PId, out IMessageSender sender))
        {
            sender.Send(new ActorMessage(MessageType.Response, context.ActorId, resp.GetOpcode(), resp, context.RpcId,
                null, context.Extra));
            return;
        }

        sender = _senderManager.GetOrCreate(context.ActorId.PId);
        if (sender is null)
        {
            Log.Error("ActorMessage发送响应失败: pid={Pid} 对应Sender不存在!", context.ActorId.PId);
        }
        else
        {
            sender.Send(new ActorMessage(MessageType.Response, context.ActorId, resp.GetOpcode(), resp, context.RpcId,
                null, context.Extra));
        }
    }
}