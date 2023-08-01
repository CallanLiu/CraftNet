using CraftNet.Services;

namespace CraftNet;


/// <summary>
/// 定时器系统
/// </summary>
public interface ITimerSystem : ISystemTypeId, ISystemBase<ITimerSystem>
{
    ITimer AddTimeout<T>(uint timeout, object state = null) where T : ITimerEvent;
    ITimer AddInterval<T>(uint interval, object state = null) where T : ITimerEvent;
}