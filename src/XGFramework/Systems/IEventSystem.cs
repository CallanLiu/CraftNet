namespace XGFramework;

public interface IEvent
{
    private static int eventIndex;

    public static class Id<T> where T : IEvent
    {
        public static readonly int Value;

        static Id()
        {
            Value = eventIndex++;
            Console.WriteLine(Value);
        }
    }
}

public interface IEventListener
{
    static abstract int EventId { get; }
}

public abstract class EventListener<T> : IEventListener where T : IEvent
{
    protected readonly App App;

    public static int EventId => IEvent.Id<T>.Value;

    protected EventListener()
    {
        App = RuntimeContext.Current;
    }

    public abstract void On(T e);
}

public interface IXEventListener
{
    void On(object arg);
}

/// <summary>
/// 事件子系统
/// </summary>
public interface IEventSystem : ISystemBase<IEventSystem>, ISystemTypeId
{
    /// <summary>
    /// 触发事件
    /// </summary>
    void Trigger<TEvent>(TEvent e) where TEvent : IEvent;

    /// <summary>
    /// 触发事件
    /// </summary>
    /// <param name="eventId"></param>
    /// <param name="arg"></param>
    void Trigger(int eventId, object arg = null);

    /// <summary>
    /// 注册事件监听器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    void RegisterListener<T>() where T : IEventListener, new();
}