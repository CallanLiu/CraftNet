using CraftNet.Services;

namespace CraftNet;

public class TimerSystemState
{
    public App App;

    private readonly Action<object> _callback;

    public TimerSystemState()
    {
        _callback = OnExecute;
    }

    /// <summary>
    /// 来自定时器线程
    /// </summary>
    /// <param name="timer"></param>
    public void OnCallback(ITimer timer) => this.App.Scheduler.Execute(_callback, timer);

    private void OnExecute(object state)
    {
        ITimer timer = (ITimer)state;
        this.App.GetSystem<IEventSystem>().Trigger(timer.Type, timer);
    }
}

public interface ITimerEvent : IEvent
{
}

/// <summary>
/// 定时器事件监听器
/// </summary>
/// <typeparam name="T"></typeparam>
public abstract class TimerEventListener<T> : IEventListener, IXEventListener where T : ITimerEvent
{
    public static EventListenerType Type => EventListenerType.Default;
    public static int EventId => IEvent.Id<T>.Value;

    public void On(object state) => On();

    protected abstract void On();
}

public abstract class TimerEventListener<T, TState> : IEventListener, IXEventListener where T : ITimerEvent
{
    public static EventListenerType Type => EventListenerType.Default;
    public static int EventId => IEvent.Id<T>.Value;

    public void On(object state)
    {
        ITimer timer = (ITimer)state;
        On((TState)timer.State);
    }

    protected abstract void On(TState state);
}

/// <summary>
/// 定时器系统
/// </summary>
public interface ITimerSystem : ISystemTypeId, ISystemBase<ITimerSystem>
{
    ITimer AddTimeout<T>(uint timeout, object state = null) where T : ITimerEvent;
    ITimer AddInterval<T>(uint interval, object state = null) where T : ITimerEvent;
}