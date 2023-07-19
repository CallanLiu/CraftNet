using CraftNet.Services;

namespace Demo.Network;

public record struct NetPacket
{
    // public NetPacket(byte MsgType, ushort Opcode, uint RpcId, object Body)
    // {
    //     this.MsgType = MsgType;
    //     this.Opcode  = Opcode;
    //     this.RpcId   = RpcId;
    //     this.Body    = Body;
    // }

    public MessageType MsgType { get; set; }
    public ushort Opcode { get; set; }
    public uint RpcId { get; set; }
    public IMessageBase Body { get; set; }

    public void Deconstruct(out MessageType msgType, out ushort opcode, out uint rpcId, out IMessageBase body)
    {
        msgType = this.MsgType;
        opcode  = this.Opcode;
        rpcId   = this.RpcId;
        body    = this.Body;
    }
}