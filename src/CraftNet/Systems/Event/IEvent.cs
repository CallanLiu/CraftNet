namespace CraftNet;

public interface IEvent
{
    private static int _eventIndex;

    public static class Id<T> where T : IEvent
    {
        public static readonly int Value;

        static Id()
        {
            Value = _eventIndex++;
        }
    }
}

/// <summary>
/// 事件监听器
/// </summary>
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

/// <summary>
/// 扩展的事件监听器
/// </summary>
public interface IEventListenerEx
{
    void On(object arg);
}