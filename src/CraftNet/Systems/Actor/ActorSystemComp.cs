using CraftNet.Services;

namespace CraftNet;

internal class ActorSystemComp : IMessageDispatcher
{
    /// <summary>
    /// Actor消息处理器
    /// </summary>
    public readonly Dictionary<ushort, (object, bool?)> MessageHandlers = new();

    public IMessageFilter[] MessageFilters;

    IMessageFilter IMessageDispatcher.GetFilter(int filterId)
    {
        if (MessageFilters is null || MessageFilters.Length <= filterId)
            return null;
        var filter = MessageFilters[filterId];
        return filter;
    }

    (object, bool?) IMessageDispatcher.GetMessageHandler(ushort opcode)
    {
        if (!MessageHandlers.TryGetValue(opcode, out ValueTuple<object, bool?> value)) return (null, null);
        return value;
    }
}