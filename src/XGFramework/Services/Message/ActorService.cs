using Serilog;

namespace XGFramework.Services;

public class ActorService : IActorService, IRpcReply
{
    private readonly ISenderManager       _senderManager;
    private readonly Slots<ActorMailbox>  _inboxes    = new();
    private readonly ReaderWriterLockSlim _rwLockSlim = new(LockRecursionPolicy.NoRecursion);
    private readonly ILocalPId            _localPId;

    public uint PId => _localPId.Value;

    public ActorService(ISenderManager senderManager, ILocalPId localPId)
    {
        _senderManager = senderManager;
        _localPId      = localPId;
    }

    public ActorId Create(object target, IAppScheduler scheduler, IMessageDispatcher dispatcher,
        bool isReentrant = false, int? filterId = null)
    {
        try
        {
            _rwLockSlim.EnterWriteLock();

            // app类型
            if (target is App app)
            {
                ActorId      actorId      = app.Id;
                ActorMailbox actorMailbox = new ActorMailbox(target, scheduler, dispatcher, isReentrant, filterId);
                _inboxes.Add(actorId.Index, actorMailbox);
                return actorId;
            }
            else // 分配
            {
                if (!_inboxes.TryAdd(null, out uint index))
                    throw new Exception("已达最大Actor数量.");

                ActorId      actorId      = new ActorId(_localPId.Value, index);
                ActorMailbox actorMailbox = new ActorMailbox(target, scheduler, dispatcher, isReentrant, filterId);
                _inboxes[index] = actorMailbox;
                return actorId;
            }
        }
        finally
        {
            _rwLockSlim.ExitWriteLock();
        }
    }

    public ActorMailbox GetMailbox(ActorId actorId)
    {
        try
        {
            _rwLockSlim.EnterReadLock();
            _inboxes.TryGet(actorId.Index, out ActorMailbox inbox);
            return inbox;
        }
        finally
        {
            _rwLockSlim.ExitReadLock();
        }
    }


    public void Post(ActorId id, byte type, ushort opcode, uint rpcId, IMessageBase body,
        IResponseCompletionSource<IResponse> tcs, int extra = 0)
    {
        ActorMessage message = new ActorMessage(type, id, opcode, body, rpcId, tcs, extra);
        this.Post(message);
    }

    public void Post(ActorMessage message)
    {
        ushort pid = message.ActorId.PId;
        if (pid == _localPId.Value) // 当前进程直接入队即可
        {
            ActorMailbox queue = this.GetMailbox(message.ActorId);
            message.RpcReply = this;
            queue.Enqueue(message);
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
                    Log.Error("ActorMessage发送失败: pid={Pid} 对应Sender不存在!", pid);
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
    public void Invoke(IResponse resp, ActorMessage context)
    {
        // Task会在其调度器中进行延续
        IResponseCompletionSource<IResponse> tcs = context.Tcs;
        tcs.Complete(resp);
    }
}