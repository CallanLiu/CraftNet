namespace CraftNet.Services;

/// <summary>
/// 消息元数据
/// </summary>
public interface IMessageMeta
{
    static abstract ushort Opcode { get; }
}

public interface IMessageBase
{
    ushort GetOpcode();
}

public interface IMessage : IMessageBase
{
}

public interface IRequest : IMessageBase
{
}

public interface IResponse : IMessageBase
{
}