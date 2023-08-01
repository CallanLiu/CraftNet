using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CraftNet;

internal class EventSystemComp
{
    public const int MaxEventTypes    = 10240;
    public const int MaxCallbackTypes = 10240;

    /// <summary>
    /// 事件处理器
    /// </summary>
    public List<object>[] EventHandlers { get; set; }

    /// <summary>
    /// 回调
    /// </summary>
    public object[] Callbacks { get; set; }
}

public class EventSystem : IEventSystem
{
    private readonly App             _app;
    private readonly EventSystemComp _comp;

    public EventSystem(App app)
    {
        _app = app;
        if (app.IsFirstLoad)
        {
            _app.AddComponent<EventSystemComp>();
        }

        _comp               = _app.GetComponent<EventSystemComp>();
        _comp.EventHandlers = Array.Empty<List<object>>();
        _comp.Callbacks     = Array.Empty<object>();
    }

    public void Trigger<TEvent>(TEvent e) where TEvent : IEvent
    {
        int eventId = IEvent.Id<TEvent>.Value;
        if (_comp.EventHandlers is null or { Length: 0 })
            return;

        if (_comp.EventHandlers.Length <= eventId)
            return;

        List<object> handlers = _comp.EventHandlers[eventId];
        if (handlers is null)
            return;

        foreach (var handler in handlers)
        {
            EventListener<TEvent> h = (EventListener<TEvent>)handler;
            h?.On(e);
        }
    }

    public void Trigger(int eventId, object arg = null)
    {
        if (_comp.EventHandlers is null or { Length: 0 })
            return;

        if (_comp.EventHandlers.Length <= eventId)
            return;

        List<object> handlers = _comp.EventHandlers[eventId];
        if (handlers is null)
            return;

        foreach (var handler in handlers)
        {
            if (handler is IEventListenerEx eventListener)
            {
                eventListener.On(arg);
            }
        }
    }

    public void RegisterListener<T>() where T : IEventListener, new()
    {
        int eventId = T.EventId;

        if (eventId > EventSystemComp.MaxEventTypes)
            throw new Exception($"最多{EventSystemComp.MaxEventTypes}种事件类型.");

        if (_comp.EventHandlers.Length <= eventId)
        {
            var arr = _comp.EventHandlers;
            Array.Resize(ref arr, eventId + 1);
            _comp.EventHandlers = arr;
        }

        List<object> handlers = _comp.EventHandlers[eventId];
        if (handlers is null)
        {
            handlers                     = new List<object>();
            _comp.EventHandlers[eventId] = handlers;
        }

        handlers.Add(ActivatorUtilities.CreateInstance<T>(_app.Services));
        Log.Debug("注册事件监听器: {Name} EventId={EventId}", typeof(T).Name, eventId);
    }

    public void RegisterCallback<T>() where T : ICallbackImpl, new()
    {
        int id = T.Id;
        if (id > EventSystemComp.MaxCallbackTypes)
            throw new Exception($"最多{EventSystemComp.MaxEventTypes}种回调类型.");

        if (_comp.Callbacks.Length <= id)
        {
            var arr = _comp.Callbacks;
            Array.Resize(ref arr, id + 1);
            _comp.Callbacks = arr;
        }

        if (_comp.Callbacks[id] != null)
            Log.Warning("注册重复的回调实现: {Name} Id={EventId}", typeof(T).Name, id);

        _comp.Callbacks[id] = ActivatorUtilities.CreateInstance<T>(_app.Services);
        Log.Debug("注册回调实现: {Name} Id={EventId}", typeof(T).Name, id);
    }

    void IEventSystem.Invoke<TCallback>(in TCallback e, object target)
    {
        int id = ICallback.Id<TCallback>.Value;
        if (_comp.Callbacks is null or { Length: 0 })
            throw new Exception($"回调实现不存在: T={typeof(TCallback).Name} id={id}");

        if (_comp.Callbacks.Length <= id)
            throw new Exception($"回调实现不存在: T={typeof(TCallback).Name} id={id}");

        object item = _comp.Callbacks[id];
        ((ICallbackImpl<TCallback>)item).On(target, e);
    }

    TResult IEventSystem.Invoke<TCallback, TResult>(in TCallback e, object target)
    {
        int id = ICallback.Id<TCallback>.Value;
        if (_comp.Callbacks is null or { Length: 0 })
            throw new Exception($"回调实现不存在: T={typeof(TCallback).Name} id={id}");

        if (_comp.Callbacks.Length <= id)
            throw new Exception($"回调实现不存在: T={typeof(TCallback).Name} id={id}");

        object item = _comp.Callbacks[id];
        return ((ICallbackImpl<TCallback, TResult>)item).On(target, e);
    }
}