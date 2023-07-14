using ProtoBuf;
using CraftNet.Services;


namespace Demo;

[ProtoContract]
public partial class G2C_EncryptKeyMsg : IMessage, IMessageMeta
{
    public static ushort Opcode => 10000;
    ushort IMessageBase.GetOpcode() => Opcode;
    [ProtoMember(1)] public int Key { get; set; }
        
}

[ProtoContract]
public partial class C2G_PingReq : IRequest, IMessageMeta
{
    public static ushort Opcode => 10001;
    ushort IMessageBase.GetOpcode() => Opcode;
        
}

[ProtoContract]
public partial class C2G_PingResp : IResponse, IMessageMeta
{
    public static ushort Opcode => 10002;
    ushort IMessageBase.GetOpcode() => Opcode;
    [ProtoMember(1)] public long ServerTime { get; set; }
        
}

[ProtoContract]
public partial class C2G_LoginGateReq : IRequest, IMessageMeta
{
    public static ushort Opcode => 10003;
    ushort IMessageBase.GetOpcode() => Opcode;
    [ProtoMember(1)] public string Token { get; set; }
        
}

[ProtoContract]
public partial class C2G_LoginGateResp : IResponse, IMessageMeta
{
    public static ushort Opcode => 10004;
    ushort IMessageBase.GetOpcode() => Opcode;
    [ProtoMember(1)] public int Err { get; set; }
        
}
