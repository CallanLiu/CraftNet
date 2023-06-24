using System.Buffers;
using System.Text.Json;
using Microsoft.AspNetCore.Connections;
using Serilog;
using XGFramework.Services;

namespace XGFramework;

public class ServerConnectionHandler : ConnectionHandler, IRpcReply
{
    private readonly IActorService      _actorService;
    private readonly ISenderManager     _senderManager;
    private readonly IMessageSerializer _messageSerializer;

    public ServerConnectionHandler(IActorService actorService, ISenderManager senderManager,
        IMessageSerializer messageSerializer)
    {
        _actorService      = actorService;
        _senderManager     = senderManager;
        _messageSerializer = messageSerializer;
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
                while (TryParse(_messageSerializer, ref buffer, out ActorMessage message))
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
    /// <param name="messageSerializer"></param>
    /// <param name="buffer"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private static bool TryParse(IMessageSerializer messageSerializer, ref ReadOnlySequence<byte> buffer,
        out ActorMessage message)
    {
        message = default;

        SequenceReader<byte> reader = new SequenceReader<byte>(buffer);
        if (!reader.TryReadBigEndian(out int length) || reader.Remaining < length + MessageHeaderHelper.LengthFieldSize)
            return false;

        // 整个消息长度
        buffer = buffer.Slice(length + MessageHeaderHelper.LengthFieldSize);

        // 头
        reader = new SequenceReader<byte>(reader.UnreadSequence.Slice(0, length));
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

        var body = messageSerializer.Deserialize(t, reader.UnreadSequence);
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