using ProtoBuf;
using XGFramework.Services;


namespace  Demo;

[ProtoContract]
public partial class C2G_PingReq : IRequest, IMessageMeta
{
    public static ushort Opcode => 10000;
    ushort IMessageBase.GetOpcode() => Opcode;
}

[ProtoContract]
public partial class C2G_PingResp : IResponse, IMessageMeta
{
    public static ushort Opcode => 10001;
    ushort IMessageBase.GetOpcode() => Opcode;
}

[ProtoContract]
public partial class C2G_TestMsg : IMessage, IMessageMeta
{
    public static ushort Opcode => 10002;
    ushort IMessageBase.GetOpcode() => Opcode;
    [ProtoMember(1)]
    public int Value { get; set; }
}