namespace CraftNet.Services;

/// <summary>
/// 消息描述
/// </summary>
public sealed class MessageDesc
{
    public readonly ushort             Opcode;
    public readonly Type               MessageType;
    public readonly IMessageSerializer Serializer;

    public MessageDesc(ushort opcode, Type messageType, IMessageSerializer serializer)
    {
        Opcode      = opcode;
        MessageType = messageType;
        Serializer  = serializer;
    }
}