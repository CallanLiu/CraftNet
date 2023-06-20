namespace XGFramework.Services;

/// <summary>
/// 消息处理器
/// </summary>
public interface IMessageHandler
{
    static abstract ushort Opcode { get; }

    ValueTask Invoke(ActorMessage context);
}

public abstract class MessageHandler<TSelf, T> : IMessageHandler where T : IMessage, IMessageMeta
{
    public static ushort Opcode => T.Opcode;

    public ValueTask Invoke(ActorMessage context)
    {
        return On((TSelf)context.Target, (T)context.Body);
    }

    protected abstract ValueTask On(TSelf self, T message);
}

public abstract class RequestHandler<TSelf, TReq, TResp> : IMessageHandler where TReq : IRequest, IMessageMeta
    where TResp : IResponse
{
    protected readonly struct Reply
    {
        private readonly ActorMessage _context;

        public Reply(ActorMessage context)
        {
            this._context = context;
        }

        public void Invoke(TResp resp) => _context.RpcReply?.Invoke(resp, _context);
    }

    public static ushort Opcode => TReq.Opcode;

    public ValueTask Invoke(ActorMessage context)
    {
        Reply r = new Reply(context);
        return On((TSelf)context.Target, (TReq)context.Body, r);
    }

    protected abstract ValueTask On(TSelf self, TReq req, Reply reply);
}