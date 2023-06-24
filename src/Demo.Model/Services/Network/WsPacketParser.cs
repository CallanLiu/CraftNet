using System.Buffers;
using System.Text.Json;
using XGFramework.Services;

namespace Demo.Network;

public record struct WsPacket(byte MsgType, ushort Opcode, uint RpcId, object Body);

public static class WsPacketParser
{
    public static bool TryParse(ref ReadOnlySequence<byte> buffer, out WsPacket packet)
    {
        packet = default;

        SequenceReader<byte> reader = new SequenceReader<byte>(buffer);

        // 消息类型
        reader.TryRead(out byte msgType);
        reader.TryReadBigEndian(out short tmp);
        packet.Opcode = (ushort)tmp;

        int jumpLength = 3; // 默认跳过消息长度即可

        if (msgType is MessageType.Request or MessageType.Response) // 没用rpcId
        {
            reader.TryReadBigEndian(out int tmpInt);
            packet.RpcId =  (uint)tmpInt;
            jumpLength   += 4;
        }

        // 获取根据opcode获取type信息.
        if (!OpcodeCollection.Ins.TryGetType(packet.Opcode, out Type t))
        {
            Console.WriteLine($"找不到opcode对应Type: opcode={packet.Opcode}");
            return false;
        }

        // 外部使用protobuf, 先用json代替。
        Utf8JsonReader jsonReader = new Utf8JsonReader(buffer.Slice(jumpLength));
        packet.MsgType = msgType;
        packet.Body    = JsonSerializer.Deserialize(ref jsonReader, t);
        return true;
    }
}