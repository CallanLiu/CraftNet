using CraftNet.Services;

namespace CraftNet;

public interface ITimerEvent : IEvent
{
}

/// <summary>
/// 定时器事件监听器
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class TimerEventListenerEx<T> : IEventListener, IEventListenerEx where T : ITimerEvent
{
    public static int EventId => IEvent.Id<T>.Value;

    public void On(object state) => On();

    protected abstract void On();
}

public abstract class TimerEventListenerEx<T, TState> : IEventListener, IEventListenerEx where T : ITimerEvent
{
    public static int EventId => IEvent.Id<T>.Value;

    public void On(object state)
    {
        ITimer timer = (ITimer)state;
        On((TState)timer.State);
    }

    protected abstract void On(TState state);
}