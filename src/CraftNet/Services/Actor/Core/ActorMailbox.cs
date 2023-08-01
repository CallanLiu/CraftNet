using Serilog;

namespace CraftNet.Services;

/// <summary>
/// 消息队列
/// </summary>
public sealed class ActorMailbox : IActor
{
    private          object              _target;
    private readonly IActorService       _actorService;
    private readonly IScheduler          _scheduler;
    private readonly Action<object>      _action;
    private readonly IMessageDispatcher  _messageDispatcher;
    private readonly Queue<ActorMessage> _queue;
    private readonly int?                _filterId;
    private          bool                _isWorking  = false;
    private          bool                _isDisposed = false;
    public ActorId ActorId { get; }

    /// <summary>
    /// Actor对象
    /// </summary>
    public object Target
    {
        get => _target;
        set => _target = value;
    }

    /// <summary>
    /// 调度器
    /// </summary>
    public IScheduler Scheduler => _scheduler;

    /// <summary>
    /// 是否可重入
    /// </summary>
    public bool IsReentrant { get; set; }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="actorService"></param>
    /// <param name="actorId"></param>
    /// <param name="scheduler">调度器</param>
    /// <param name="messageDispatcher">消息分发器</param>
    /// <param name="isReentrant">是否消息重入</param>
    /// <param name="filterId">拦截器</param>
    public ActorMailbox(IActorService actorService, ActorId actorId,
        IScheduler scheduler,
        IMessageDispatcher messageDispatcher,
        bool isReentrant,
        int? filterId)
    {
        ActorId = actorId;

        _actorService      = actorService;
        _scheduler         = scheduler;
        _action            = OnRun;
        _messageDispatcher = messageDispatcher;
        _queue             = new Queue<ActorMessage>();
        IsReentrant        = isReentrant;
        _filterId          = filterId;
    }

    /// <summary>
    /// 入队消息(线程安全)
    /// </summary>
    /// <param name="actorMessage"></param>
    public void Post(ActorMessage actorMessage)
    {
        if (RuntimeContext.Current?.Scheduler == this._scheduler)
        {
            _action.Invoke(actorMessage);
        }
        else
        {
            _scheduler.Execute(_action, actorMessage);
        }
    }

    /// <summary>
    /// 入队消息(此方法线程不安全)
    /// </summary>
    /// <param name="actorMessage"></param>
    public void UnsafePost(ActorMessage actorMessage) => _action.Invoke(actorMessage);

    /// <summary>
    /// 调度到逻辑线程
    /// </summary>
    /// <param name="state"></param>
    private async void OnRun(object state)
    {
        ActorMessage msg = (ActorMessage)state;
        msg.RpcReply ??= _actorService; // 如果外部没有指定，将使用本地的rpc回应处理。

        _queue.Enqueue(msg);
        if (_isWorking) // await 执行权被切出去时，只入队。
            return;
        _isWorking = true;

        while (_queue.TryDequeue(out ActorMessage actorMessage))
        {
            try
            {
                actorMessage.RpcReply ??= _actorService;
                actorMessage.Target   =   this._target;

                (object messageHandler, bool? alwaysInterleave) =
                    _messageDispatcher.GetMessageHandler(actorMessage.Opcode);

                bool isInterleave = IsReentrant;                         // 默认
                if (messageHandler != null && alwaysInterleave.HasValue) // 每个处理器单独设置的
                    isInterleave = alwaysInterleave.Value;

                ValueTask valueTask = InvokeIncoming((IMessageHandler)messageHandler, actorMessage);
                if (!isInterleave)
                    await valueTask;
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

        _isWorking = false;
    }

    private async ValueTask InvokeIncoming(IMessageHandler messageHandler, ActorMessage actorMessage)
    {
        if (_filterId.HasValue) // 走拦截器
        {
            IMessageFilter messageFilter = _messageDispatcher.GetFilter(_filterId.Value);
            if (messageFilter != null)
            {
                // 有拦截器，就不会自动分发消息了。
                MessageFilterContext messageFilterContext = new MessageFilterContext(actorMessage, messageHandler);
                bool                 isContinue           = await messageFilter.On(messageFilterContext);
                if (!isContinue)
                    return;
            }
        }

        if (messageHandler != null)
        {
            await messageHandler.Invoke(actorMessage);
        }
        else
        {
            Log.Error("消息处理器不存在: Opcode={Opcode}", actorMessage.Opcode);
        }
    }

    private void OnDestroy()
    {
        if (_isDisposed)
            return;
        _isDisposed = true;
        _actorService.Destroy(this.ActorId);
    }

    /// <summary>
    /// 释放这个Actor(把释放操作投递到调度器中，保证释放后，不会进行消息处理)
    /// </summary>
    public void Dispose() => this._scheduler.Execute(OnDestroy);
}