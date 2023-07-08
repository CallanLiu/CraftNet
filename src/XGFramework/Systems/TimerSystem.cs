using Microsoft.Extensions.DependencyInjection;
using XGFramework.Services;

namespace XGFramework;

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