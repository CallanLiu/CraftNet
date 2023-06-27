using Microsoft.Extensions.DependencyInjection;
using XGFramework.Services;

namespace XGFramework;

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
    public static int EventId => IEvent.Id<T>.Value;

    public void On(object state) => On();

    protected abstract void On();
}

public abstract class TimerEventListener<T, TState> : IEventListener, IXEventListener where T : ITimerEvent
{
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

public class TimerSystem : ITimerSystem
{
    private readonly ITimerService    _timerService;
    private readonly TimerSystemState _state;

    public TimerSystem(App app)
    {
        if (app.IsFirstLoad)
        {
            var state = app.AddState<TimerSystemState>();
            state.App = app;
        }

        _state        = app.GetState<TimerSystemState>();
        _timerService = app.Services.GetService<ITimerService>();
    }

    public ITimer AddTimeout<T>(uint timeout, object state = null) where T : ITimerEvent
    {
        int type = IEvent.Id<T>.Value;
        return _timerService.AddTimeout(type, timeout, _state.OnCallback, state);
    }

    public ITimer AddInterval<T>(uint interval, object state = null) where T : ITimerEvent
    {
        int type = IEvent.Id<T>.Value;
        return _timerService.AddInterval(type, interval, _state.OnCallback, state);
    }
}