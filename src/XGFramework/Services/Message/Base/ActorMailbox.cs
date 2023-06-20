using Serilog;

namespace XGFramework.Services;

/// <summary>
/// 消息队列
/// </summary>
public sealed class ActorMailbox
{
    private readonly object              _target;
    private readonly IAppScheduler       _scheduler;
    private readonly Action<object>      _action;
    private readonly IMessageDispatcher  _dispatcher;
    private readonly Queue<ActorMessage> _queue;
    private readonly bool                _isReentrant;
    private readonly int?                _filterId;
    private          bool                _isWorking = false;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target"></param>
    /// <param name="scheduler"></param>
    /// <param name="dispatcher"></param>
    /// <param name="isReentrant">是否消息重入</param>
    /// <param name="filterId"></param>
    public ActorMailbox(object target, IAppScheduler scheduler, IMessageDispatcher dispatcher, bool isReentrant,
        int? filterId)
    {
        this._target    = target;
        this._scheduler = scheduler;
        _action         = OnRun;
        _dispatcher     = dispatcher;
        _queue          = new Queue<ActorMessage>();
        _isReentrant    = isReentrant;
        _filterId       = filterId;
    }

    public void Enqueue(ActorMessage actorMessage) => this._scheduler.Execute(_action, actorMessage);

    /// <summary>
    /// 调度到逻辑线程
    /// </summary>
    /// <param name="state"></param>
    private async void OnRun(object state)
    {
        _queue.Enqueue((ActorMessage)state);
        if (_isWorking) // await 执行权被切出去时，只入队。
            return;
        _isWorking = true;

        while (_queue.TryDequeue(out ActorMessage messageContext))
        {
            ValueTask valueTask = InvokeIncoming(messageContext);
            if (!_isReentrant)
                await valueTask;
        }

        _isWorking = false;
    }

    private async ValueTask InvokeIncoming(ActorMessage actorMessage)
    {
        try
        {
            actorMessage.Target = this._target;
            await _dispatcher.Dispatch(_filterId, actorMessage);
        }
        catch (Exception e)
        {
            Log.Error(e, "消息处理:");
        }
        finally
        {
            // 回收上下文
        }
    }
}