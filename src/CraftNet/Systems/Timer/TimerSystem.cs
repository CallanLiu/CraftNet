using CraftNet.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CraftNet;

public class TimerSystem : ITimerSystem
{
    private readonly ITimerService    _timerService;
    private readonly TimerSystemComp _comp;

    public TimerSystem(App app)
    {
        if (app.IsFirstLoad)
        {
            var state = app.AddComponent<TimerSystemComp>();
            state.App = app;
        }

        _comp        = app.GetComponent<TimerSystemComp>();
        _timerService = app.Services.GetService<ITimerService>();
    }

    public ITimer AddTimeout<T>(uint timeout, object state = null) where T : ITimerEvent
    {
        int type = IEvent.Id<T>.Value;
        return _timerService.AddTimeout(type, timeout, _comp.OnCallback, state);
    }

    public ITimer AddInterval<T>(uint interval, object state = null) where T : ITimerEvent
    {
        int type = IEvent.Id<T>.Value;
        return _timerService.AddInterval(type, interval, _comp.OnCallback, state);
    }
}