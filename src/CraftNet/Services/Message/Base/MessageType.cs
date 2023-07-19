namespace CraftNet.Services;

public enum MessageType : byte
{
    Message           = 1,
    Request           = 1 << 1,
    Response          = 1 << 2,
    ExceptionResponse = Response | 1 << 3 // 异常响应
}

public static class MessageTypeExtensions
{
    public static bool HasRpcField(this MessageType self)
    {
        return self is MessageType.Request or MessageType.Response or MessageType.ExceptionResponse;
    }
}