using System.Buffers.Binary;
using System.IO.Pipelines;
using XGFramework.Services;

namespace XGFramework;

/// <summary>
/// 内部网络消息: length[4] + actorId[8] + type[1] + opcode[2] + rpcId[4] + body ...
/// length都要算,但不包括自己.
/// </summary>
public static class MessageHeaderHelper
{
    /// <summary>
    /// 最小Size(不包括rpcId)
    /// </summary>
    public const int MinSize = 4 + 8 + 1 + 2;

    /// <summary>
    /// Length字段大小
    /// </summary>
    public const int LengthFieldSize = 4;

    public const int RpcIdFieldSize = 4;

    /// <summary>
    /// 写入消息头
    /// </summary>
    /// <param name="message"></param>
    /// <param name="output"></param>
    /// <param name="actorId"></param>
    public static Span<byte> WriteHead(ActorMessage message, PipeWriter output, ActorId actorId)
    {
        int size = MinSize + message.Type is MessageType.Request ? RpcIdFieldSize : 0;

        // 分配
        Span<byte> headSpan = output.GetSpan(size);

        int pos = 0;
        BinaryPrimitives.WriteInt32BigEndian(headSpan, 0); // len 占位
        pos += 4;

        BinaryPrimitives.WriteUInt64BigEndian(headSpan[pos..], actorId);
        pos += 8;

        headSpan[pos] =  message.Type;
        pos           += 1;

        BinaryPrimitives.WriteUInt16BigEndian(headSpan[pos..], message.Opcode);
        pos += 2;

        if (message.Type is MessageType.Request or MessageType.Response)
        {
            BinaryPrimitives.WriteUInt32BigEndian(headSpan[pos..], message.RpcId);
            pos += 4;
        }

        output.Advance(pos);
        return headSpan;
    }
}