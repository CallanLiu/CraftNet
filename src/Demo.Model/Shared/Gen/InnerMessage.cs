using MemoryPack;
using XGFramework.Services;


namespace  Demo;

[MemoryPackable]
public partial class Login2G_GetTokenReq : IRequest, IMessageMeta
{
    public static ushort Opcode => 1;
    ushort IMessageBase.GetOpcode() => Opcode;
    public string Account { get; set; }
}

[MemoryPackable]
public partial class Login2G_GetTokenResp : IResponse, IMessageMeta
{
    public static ushort Opcode => 2;
    ushort IMessageBase.GetOpcode() => Opcode;
    public long Token { get; set; }
}