namespace XGFramework;

public class EventSystemComp : IComp
{
    public List<object>[] EventHandlers { get; set; }
}

public class EventSystem : IEventSystem
{
    private readonly App             _app;
    private readonly EventSystemComp _comp;

    public EventSystem(App app)
    {
        _app = app;
        if (app.IsFirstLoad)
        {
            _app.AddComp<EventSystemComp>();
        }

        _comp = _app.GetComp<EventSystemComp>();

        _comp.EventHandlers = Array.Empty<List<object>>();
    }

    public void Trigger<TEvent>(TEvent e) where TEvent : IEvent
    {
        int eventId = IEvent.Id<TEvent>.Value;
        if (_comp.EventHandlers is null or { Length: 0 })
            return;

        if (_comp.EventHandlers.Length <= eventId)
            return;

        List<object> handlers = _comp.EventHandlers[eventId];
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
        if (_comp.EventHandlers.Length <= eventId)
        {
            var arr = _comp.EventHandlers;
            Array.Resize(ref arr, eventId + 1);
            _comp.EventHandlers = arr;
        }

        List<object> handlers = _comp.EventHandlers[eventId];
        if (handlers is null)
        {
            handlers                     = new List<object>();
            _comp.EventHandlers[eventId] = handlers;
        }

        handlers.Add(new T());
        Console.WriteLine($"注册事件监听器: {typeof(T).Name} e={eventId}");
    }
}