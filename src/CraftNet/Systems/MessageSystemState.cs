using CraftNet.Services;
using Serilog;

namespace CraftNet;

public class MessageSystemState : IMessageDispatcher
{
    public readonly  Dictionary<ushort, object>    Handlers = new();
    public           IMessageFilter[]              MessageFilters;
    private readonly Func<ActorMessage, ValueTask> _func;

    public MessageSystemState()
    {
        _func = Dispatch;
    }

    private IMessageFilter GetFilter(int filterId)
    {
        if (MessageFilters is null || MessageFilters.Length <= filterId)
            return null;
        var filter = MessageFilters[filterId];
        return filter;
    }

    public async ValueTask Dispatch(int? filterId, ActorMessage actorMessage)
    {
        // 有拦截器
        if (filterId.HasValue)
        {
            IMessageFilter messageFilter = GetFilter(filterId.Value);
            if (messageFilter is null)
            {
            }
            else
            {
                // 有拦截器，就不会自动分发消息了。
                MessageFilterContext messageFilterContext = new MessageFilterContext(actorMessage, _func);
                bool                 isContinue           = await messageFilter.On(messageFilterContext);
                if (!isContinue)
                    return;
            }
        }

        await Dispatch(actorMessage);
    }

    private ValueTask Dispatch(ActorMessage actorMessage)
    {
        if (Handlers.TryGetValue(actorMessage.Opcode, out object messageHandler))
        {
            IMessageHandler handler = (IMessageHandler)messageHandler;
            return handler.Invoke(actorMessage);
        }

        Log.Warning("消息处理器不存在: opcode={Opcode}", actorMessage.Opcode);
        return ValueTask.CompletedTask;
    }
}