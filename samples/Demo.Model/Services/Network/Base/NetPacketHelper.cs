using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using CraftNet;
using Demo.Network;
using Serilog;
using CraftNet.Services;

namespace Demo;

public static class NetPacketHelper
{
    public static bool TryParse(IMessageDescCollection messageDescCollection, ref ReadOnlySequence<byte> buffer,
        ref XRandom random,
        out NetPacket packet)
    {
        packet = default;

        SequenceReader<byte> reader = new SequenceReader<byte>(buffer);

        // 消息类型
        reader.TryRead(out byte tmpMsgType);
        reader.TryReadLittleEndian(out short tmp);
        // packet.Opcode = (ushort)(tmp ^ random.RandomInt());
        packet.Opcode = (ushort)tmp;

        MessageType msgType = (MessageType)tmpMsgType;
        if (msgType.HasRpcField()) // 没用rpcId
        {
            reader.TryReadLittleEndian(out uint tmpInt);
            packet.RpcId = tmpInt;
        }

        // 获取根据opcode获取type信息.
        if (!messageDescCollection.TryGet(packet.Opcode, out MessageDesc messageDesc))
        {
            Log.Error("找不到opcode对应Type: opcode={Opcode}", packet.Opcode);
            return false;
        }

        packet.MsgType = msgType;
        IMessageBase messageBase =
            (IMessageBase)messageDesc.Serializer.Deserialize(messageDesc.MessageType, reader.UnreadSequence);
        packet.Body = messageBase;
        return true;
    }

    public static void Send(IMessageDescCollection messageDescCollection, PipeWriter output, in WaitSendPacket packet)
    {
        if (!messageDescCollection.TryGet(packet.Opcode, out MessageDesc messageDesc))
            return;

        // ushort op = (ushort)(packet.Opcode ^ packet.Key);
        ushort op = packet.Opcode;

        // 发给客户端只有两种类型(IMessage与IResponse)
        if (packet.RpcId is null)
        {
            // 写入头: type[1]+opcode[2]
            Memory<byte> buffer = output.GetMemory(3);
            buffer.Span[0] = (byte)MessageType.Message;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Span[1..], op);
            output.Advance(3);
        }
        else
        {
            // 写入头: type[1]+opcode[2]+rpcId[4]
            Memory<byte> buffer = output.GetMemory(7);
            buffer.Span[0] = (byte)MessageType.Response;
            BinaryPrimitives.WriteUInt16LittleEndian(buffer.Span.Slice(1, 2), op);
            BinaryPrimitives.WriteUInt32LittleEndian(buffer.Span.Slice(3, 4), packet.RpcId.Value);
            output.Advance(7);
        }

        messageDesc.Serializer.Serialize(packet.Body, output);
    }
}