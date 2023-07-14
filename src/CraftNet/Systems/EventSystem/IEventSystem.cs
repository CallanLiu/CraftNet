namespace CraftNet;

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

    /// <summary>
    /// 注册回调
    /// </summary>
    /// <typeparam name="T"></typeparam>
    void RegisterCallback<T>() where T : ICallbackImpl<T>, new();

    /// <summary>
    /// 调用无返回值的回调
    /// </summary>
    /// <param name="e"></param>
    /// <param name="target">目标</param>
    /// <typeparam name="TCallback"></typeparam>
    void Invoke<TCallback>(in TCallback e, object target = null) where TCallback : ICallback;

    /// <summary>
    /// 调用有返回值的回调
    /// </summary>
    /// <param name="e"></param>
    /// <param name="target">目标</param>
    /// <typeparam name="TCallback"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    TResult Invoke<TCallback, TResult>(in TCallback e, object target = null) where TCallback : ICallback<TResult>;
}