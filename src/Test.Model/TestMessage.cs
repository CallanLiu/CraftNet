using XGFramework.Services;

namespace Test;

public class TestMessage : IMessage, IMessageMeta
{
    public static ushort Opcode => 1;
    public ushort GetOpcode() => Opcode;
}

public class PingReq : IRequest, IMessageMeta
{
    public static ushort Opcode => 2;
    public ushort GetOpcode() => Opcode;
}

public class PingResp : IResponse, IMessageMeta
{
    public static ushort Opcode => 3;
    public ushort GetOpcode() => Opcode;

    public string Msg { get; set; }
}

public class TestWsMessage : IMessage, IMessageMeta
{
    public static ushort Opcode => 4;
    public ushort GetOpcode() => Opcode;

    public string Text { get; set; }
}

public class PingAppReq : IRequest, IMessageMeta
{
    public static ushort Opcode => 5;
    public ushort GetOpcode() => Opcode;
}

public class PingAppResp : IResponse, IMessageMeta
{
    public static ushort Opcode => 6;
    public ushort GetOpcode() => Opcode;

    public string Msg { get; set; }
}