using ProtoBuf;
using CraftNet.Services;


namespace Demo;

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
    [ProtoMember(1)] public long ServerTime { get; set; }
        
}

[ProtoContract]
public partial class C2G_LoginReq : IRequest, IMessageMeta
{
    public static ushort Opcode => 10002;
    ushort IMessageBase.GetOpcode() => Opcode;
    [ProtoMember(1)] public string Account { get; set; }
    [ProtoMember(2)] public string Password { get; set; }
        
}

[ProtoContract]
public partial class C2G_LoginResp : IResponse, IMessageMeta
{
    public static ushort Opcode => 10003;
    ushort IMessageBase.GetOpcode() => Opcode;
    [ProtoMember(1)] public int ErrCode { get; set; }
        
}

[ProtoContract]
public partial class C2G_RegisterReq : IRequest, IMessageMeta
{
    public static ushort Opcode => 10004;
    ushort IMessageBase.GetOpcode() => Opcode;
    [ProtoMember(1)] public string Account { get; set; }
    [ProtoMember(2)] public string Password { get; set; }
        
}

[ProtoContract]
public partial class C2G_RegisterResp : IResponse, IMessageMeta
{
    public static ushort Opcode => 10005;
    ushort IMessageBase.GetOpcode() => Opcode;
    [ProtoMember(1)] public int ErrCode { get; set; }
        
}
