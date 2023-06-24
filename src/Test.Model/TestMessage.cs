using MemoryPack;
using ProtoBuf;
using XGFramework.Services;

namespace Test;

[ProtoContract]
[MemoryPackable]
public partial class PingAppReq : IRequest, IMessageMeta
{
    public static ushort Opcode => 5;
    public ushort GetOpcode() => Opcode;
}

[ProtoContract]
[MemoryPackable]
public partial class PingAppResp : IResponse, IMessageMeta
{
    public static ushort Opcode => 6;
    public ushort GetOpcode() => Opcode;

    [ProtoMember(1)]
    public string Msg { get; set; }
}