using System.Buffers;
using CraftNet.Services;
using Microsoft.AspNetCore.Connections;
using Serilog;

namespace CraftNet;

public class ServerConnectionHandler : ConnectionHandler, IRpcReply
{
    private readonly IActorService          _actorService;
    private readonly ISenderManager         _senderManager;
    private readonly IMessageDescCollection _messageDescCollection;

    public ServerConnectionHandler(IActorService actorService, ISenderManager senderManager,
        IMessageDescCollection messageDescCollection)
    {
        _actorService          = actorService;
        _senderManager         = senderManager;
        _messageDescCollection = messageDescCollection;
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
                while (TryParse(_messageDescCollection, ref buffer, out ActorMessage message))
                {
                    if (message.Type.HasResponse())
                    {
                        if (_senderManager.TryGet(message.ActorId.PId, out var sender))
                        {
                            sender.OnResponse(message);
                        }
                    }
                    else
                    {
                        if (message.Type.HasRequest())
                            message.RpcReply = this;

                        _actorService.Post(message);
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
    /// <param name="messageDescCollection"></param>
    /// <param name="buffer"></param>
    /// <param name="message"></param>
    /// <returns></returns>
    private static bool TryParse(IMessageDescCollection messageDescCollection, ref ReadOnlySequence<byte> buffer,
        out ActorMessage message)
    {
        message = default;

        if (!LengthFieldBasedFrameDecoder.TryParse(ref buffer, out uint length, out SequenceReader<byte> reader))
            return false;

        // 直接切割到这个消息的结束位置
        buffer = buffer.Slice(length + 4);

        // actorId
        reader.TryReadLittleEndian(out ushort actorPId); // 远端Pid
        reader.TryReadLittleEndian(out uint actorType);
        reader.TryReadLittleEndian(out long actorKey);
        ActorId actorId = new ActorId(actorPId, actorType, actorKey);

        reader.TryReadLittleEndian(out ushort senderPId);

        // 消息信息
        reader.TryRead(out byte tmpType);
        MessageType type = (MessageType)tmpType;
        reader.TryReadLittleEndian(out ushort opcode);
        uint rpcId = 0;
        if (type.HasRpcField())
        {
            reader.TryReadLittleEndian(out uint tmpRpcId);
            rpcId = tmpRpcId;
        }

        if (!messageDescCollection.TryGet(opcode, out MessageDesc messageDesc))
        {
            Console.WriteLine($"找不到opcode对应Type: opcode={opcode}");
            return false;
        }

        var body = messageDesc.Serializer.Deserialize(messageDesc.MessageType, reader.UnreadSequence);
        message = new ActorMessage(type, actorId, opcode, (IMessageBase)body, rpcId)
        {
            SenderPId = senderPId
        };

        return true;
    }

    void IRpcReply.OnRpcReply(IResponse resp, ActorMessage context)
    {
        if (_senderManager.TryGet(context.SenderPId, out IMessageSender sender))
        {
            sender.Send(ActorMessage.CreateResponse(context.ActorId, resp.GetOpcode(), context.RpcId, resp,
                context.Direction));
            return;
        }

        sender = _senderManager.GetOrCreate(context.SenderPId);
        if (sender is null)
        {
            Log.Error("ActorMessage发送响应失败: pid={Pid} 对应Sender不存在!", context.ActorId.PId);
        }
        else
        {
            sender.Send(ActorMessage.CreateResponse(context.ActorId, resp.GetOpcode(), context.RpcId, resp,
                context.Direction));
        }
    }
}