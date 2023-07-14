using Serilog;

namespace CraftNet.Services;

/// <summary>
/// 消息队列
/// </summary>
public sealed class ActorMailbox
{
    private readonly object                  _target;
    private readonly IApp                    _app;
    private readonly Action<object>          _action;
    private readonly IActorMessageDispatcher _messageDispatcher;
    private readonly Queue<ActorMessage>     _queue;
    private readonly int?                    _filterId;
    private          bool                    _isReentrant;
    private          bool                    _isWorking = false;

    /// <summary>
    /// Actor对象
    /// </summary>
    public object Target => _target;

    /// <summary>
    /// 所在App
    /// </summary>
    public IApp App => _app;

    /// <summary>
    /// 是否可重入
    /// </summary>
    public bool IsReentrant
    {
        get => _isReentrant;
        set => _isReentrant = value;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="target">目标对象</param>
    /// <param name="app">调度器</param>
    /// <param name="messageDispatcher">消息分发器</param>
    /// <param name="isReentrant">是否消息重入</param>
    /// <param name="filterId">拦截器</param>
    public ActorMailbox(object target, IApp app,
        IActorMessageDispatcher messageDispatcher,
        bool isReentrant,
        int? filterId)
    {
        this._target       = target;
        this._app          = app;
        _action            = OnRun;
        _messageDispatcher = messageDispatcher;
        _queue             = new Queue<ActorMessage>();
        _isReentrant       = isReentrant;
        _filterId          = filterId;
    }

    /// <summary>
    /// 入队消息(线程安全)
    /// </summary>
    /// <param name="actorMessage"></param>
    public void Enqueue(ActorMessage actorMessage) => this._app.Scheduler.Execute(_action, actorMessage);

    /// <summary>
    /// 入队消息(此方法线程不安全)
    /// </summary>
    /// <param name="actorMessage"></param>
    public void UnsafeEnqueue(ActorMessage actorMessage) => _action.Invoke(actorMessage);

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
            await _messageDispatcher.Dispatch(_filterId, actorMessage);
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