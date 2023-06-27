using System.Buffers;
using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Text.Json;
using XGFramework.Services;

namespace Demo.Network;

public record struct WsPacket
{
    // public WsPacket(byte MsgType, ushort Opcode, uint RpcId, object Body)
    // {
    //     this.MsgType = MsgType;
    //     this.Opcode  = Opcode;
    //     this.RpcId   = RpcId;
    //     this.Body    = Body;
    // }

    public byte MsgType { get; set; }
    public ushort Opcode { get; set; }
    public uint RpcId { get; set; }
    public object Body { get; set; }

    public void Deconstruct(out byte msgType, out ushort opcode, out uint rpcId, out object body)
    {
        msgType = this.MsgType;
        opcode  = this.Opcode;
        rpcId   = this.RpcId;
        body    = this.Body;
    }

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

    public static void Send(PipeWriter output, in WaitSendPacket packet)
    {
        // 发给客户端只有两种类型(IMessage与IResponse)
        if (packet.RpcId is null)
        {
            // 写入头: type[1]+opcode[2]
            Memory<byte> buffer = output.GetMemory(3);
            buffer.Span[0] = MessageType.Message;
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Span[1..], packet.Opcode);
            output.Advance(3);

            // 写入body
            Utf8JsonWriter jsonWriter = new Utf8JsonWriter(output);
            JsonSerializer.Serialize(jsonWriter, packet.Body);
        }
        else
        {
            // 写入头: type[1]+opcode[2]+rpcId[4]
            Memory<byte> buffer = output.GetMemory(7);
            buffer.Span[0] = MessageType.Response;
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Span.Slice(1, 2), packet.Opcode);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Span.Slice(3, 4), packet.RpcId.Value);
            output.Advance(7);

            // 写入body
            Utf8JsonWriter jsonWriter = new Utf8JsonWriter(output);
            JsonSerializer.Serialize(jsonWriter, packet.Body);
        }
    }
}