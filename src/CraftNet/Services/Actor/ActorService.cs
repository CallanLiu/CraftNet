using Serilog;

namespace CraftNet.Services;

public class ActorService : IActorService
{
    private readonly ISenderManager                    _senderManager;
    private readonly Dictionary<ActorId, ActorMailbox> _inboxes    = new();
    private readonly ReaderWriterLockSlim              _rwLockSlim = new(LockRecursionPolicy.NoRecursion);
    private readonly ILocalPId                         _localPId;

    public ActorService(ISenderManager senderManager, ILocalPId localPId)
    {
        _senderManager = senderManager;
        _localPId      = localPId;
    }

    public IActor Create(uint type, long key, IScheduler scheduler, IMessageDispatcher messageDispatcher,
        bool isReentrant = false, int? filterId = null)
    {
        try
        {
            _rwLockSlim.EnterWriteLock();

            ActorId actorId = new ActorId(_localPId.Value, type, key);
            if (_inboxes.ContainsKey(actorId))
                throw new ArgumentException($"actor已经存在: {actorId}");
            ActorMailbox actorMailbox =
                new ActorMailbox(this, actorId, scheduler, messageDispatcher, isReentrant, filterId);
            _inboxes.Add(actorId, actorMailbox);
            return actorMailbox;
        }
        finally
        {
            _rwLockSlim.ExitWriteLock();
        }
    }

    public IActor Get(uint type, long key)
    {
        try
        {
            _rwLockSlim.EnterReadLock();
            _inboxes.TryGetValue(new ActorId(_localPId.Value, type, key), out var actorMailbox);
            return actorMailbox;
        }
        finally
        {
            _rwLockSlim.ExitReadLock();
        }
    }

    /// <summary>
    /// 移除掉就行(只能从IActor中调用)
    /// </summary>
    /// <param name="actorId"></param>
    void IActorService.Destroy(ActorId actorId)
    {
        try
        {
            _rwLockSlim.EnterWriteLock();
            _inboxes.Remove(actorId);
        }
        finally
        {
            _rwLockSlim.ExitWriteLock();
        }
    }

    public void Post(ActorMessage message)
    {
        ushort pid = message.ActorId.PId;
        if (pid == _localPId.Value) // 当前进程直接入队即可
        {
            IActor actor = this.Get(message.ActorId.Type, message.ActorId.Key);
            if (actor is null)
            {
                if (message.Type is MessageType.Request)
                {
                    // 回应异常ActorNotFound
                    message.RpcReply ??= this;
                    message.RpcReply.OnRpcReply(new ExceptionResponse($"Actor不存在: {message.ActorId.ToString()}"),
                        message);
                }
                else if (message.Type is MessageType.Message)
                {
                    // 直接打印
                    Log.Warning("Actor不存在: {ActorId}", message.ActorId.ToString());
                }

                return;
            }

            actor.Post(message);
        }
        else // 投递到另一个进程
        {
            // 通过PID取得连接
            if (_senderManager.TryGet(pid, out IMessageSender sender))
            {
                sender.Send(message);
            }
            else
            {
                sender = _senderManager.GetOrCreate(pid);
                if (sender is null)
                {
                    Log.Error("ActorMessage发送失败: {ActorId} 对应Sender不存在!", message.ActorId.ToString());
                }
                else
                {
                    sender.Send(message);
                }
            }
        }
    }

    /// <summary>
    /// 执行线程在消息队列中
    /// </summary>
    /// <param name="resp"></param>
    /// <param name="context"></param>
    public void OnRpcReply(IResponse resp, ActorMessage context)
    {
        // Task会在其调度器中进行延续
        IResponseCompletionSource<IResponse> tcs = context.Tcs;
        tcs.Complete(resp);
    }
}