namespace CraftNet.Services;

/// <summary>
/// 定时器回调
/// </summary>
public delegate void TimerCallback(ITimer timer);

/// <summary>
/// 定时器服务
/// </summary>
public interface ITimerService
{
    ITimer AddTimeout(int type, uint timeout, TimerCallback action, object state);
    ITimer AddInterval(int type, uint interval, TimerCallback action, object state);
}