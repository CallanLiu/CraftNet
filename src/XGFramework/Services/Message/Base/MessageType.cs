namespace XGFramework.Services;

public static class MessageType
{
    public const byte Message  = 1;
    public const byte Request  = 1 << 1;
    public const byte Response = 1 << 2;
}