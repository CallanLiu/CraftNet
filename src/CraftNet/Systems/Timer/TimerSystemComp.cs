using CraftNet.Services;

namespace CraftNet;

public class TimerSystemComp
{
    public App App;

    private readonly Action<object> _callback;

    public TimerSystemComp()
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