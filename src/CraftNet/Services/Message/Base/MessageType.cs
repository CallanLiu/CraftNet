using System.Runtime.CompilerServices;

namespace CraftNet.Services;

public enum MessageType : byte
{
    Message  = 1,
    Request  = 1 << 1,
    Response = 1 << 2,
}

public static class MessageTypeExtensions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasRpcField(this MessageType self)
    {
        return self is MessageType.Request or MessageType.Response;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasResponse(this MessageType self)
    {
        return self is MessageType.Response;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static bool HasRequest(this MessageType self)
    {
        return self is MessageType.Request;
    }
}