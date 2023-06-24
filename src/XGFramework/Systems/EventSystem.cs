namespace XGFramework;

public class EventSystemState
{
    public List<object>[] EventHandlers { get; set; }
}

public class EventSystem : IEventSystem
{
    private readonly App             _app;
    private readonly EventSystemState _state;

    public EventSystem(App app)
    {
        _app = app;
        if (app.IsFirstLoad)
        {
            _app.AddState<EventSystemState>();
        }

        _state = _app.GetState<EventSystemState>();

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
            handlers                     = new List<object>();
            _state.EventHandlers[eventId] = handlers;
        }

        handlers.Add(new T());
        Console.WriteLine($"注册事件监听器: {typeof(T).Name} e={eventId}");
    }
}