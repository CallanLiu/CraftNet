using Serilog;

namespace CraftNet;

public class EventSystemState
{
    public List<object>[] EventHandlers { get; set; }
}

public class EventSystem : IEventSystem
{
    private readonly App              _app;
    private readonly EventSystemState _state;

    public EventSystem(App app)
    {
        _app = app;
        if (app.IsFirstLoad)
        {
            _app.AddComponent<EventSystemState>();
        }

        _state = _app.GetComponent<EventSystemState>();

        _state.EventHandlers = Array.Empty<List<object>>();
    }

    public void Trigger<TEvent>(TEvent e) where TEvent : IEvent
    {
        int eventId = IEvent.Id<TEvent>.Value;
        if (_state.EventHandlers is null or { Length: 0 })
            return;

        if (_state.EventHandlers.Length <= eventId)
            return;

        List<object> handlers = _state.EventHandlers[eventId];
        if (handlers is null)
            return;

        foreach (var handler in handlers)
        {
            EventListener<TEvent> h = (EventListener<TEvent>)handler;
            h?.On(e);
        }
    }

    public void Trigger(int eventId, object arg = null)
    {
        if (_state.EventHandlers is null or { Length: 0 })
            return;

        if (_state.EventHandlers.Length <= eventId)
            return;

        List<object> handlers = _state.EventHandlers[eventId];
        if (handlers is null)
            return;

        foreach (var handler in handlers)
        {
            if (handler is IXEventListener eventListener)
            {
                eventListener.On(arg);
            }
        }
    }

    public void RegisterListener<T>() where T : IEventListener, new()
    {
        int eventId = T.EventId;

        if (eventId > 10240)
            throw new Exception("最多10240种事件类型.");

        if (_state.EventHandlers.Length <= eventId)
        {
            var arr = _state.EventHandlers;
            Array.Resize(ref arr, eventId + 1);
            _state.EventHandlers = arr;
        }

        List<object> handlers = _state.EventHandlers[eventId];
        if (handlers is null)
        {
            handlers                      = new List<object>();
            _state.EventHandlers[eventId] = handlers;
        }

        if (T.Type is EventListenerType.Invokable && handlers.Count > 0)
        {
            object prev = handlers.First();
            handlers.Clear();
            Log.Error("注册Invokable错误: {TypeName} 已经存在相同的 {PrevName} ...", typeof(T).Name, prev.GetType().Name);
        }

        handlers.Add(new T());
        Log.Debug("注册事件监听器: {Name} EventId={EventId}", typeof(T).Name, eventId);
    }

    public TResult Invoke<TEvent, TResult>(TEvent e) where TEvent : IEvent
    {
        int eventId = IEvent.Id<TEvent>.Value;
        if (_state.EventHandlers is null or { Length: 0 })
            throw new Exception($"事件处理器不存在: e={typeof(TEvent)}");

        if (_state.EventHandlers.Length <= eventId)
            throw new Exception($"事件处理器不存在: e={typeof(TEvent)}");

        List<object> handlers = _state.EventHandlers[eventId];
        if (handlers is null or { Count: 0 })
            throw new Exception($"事件处理器不存在: e={typeof(TEvent)}");

        object                          item = handlers.First();
        EventInvokable<TEvent, TResult> h    = (EventInvokable<TEvent, TResult>)item;
        return h.On(e);
    }
}