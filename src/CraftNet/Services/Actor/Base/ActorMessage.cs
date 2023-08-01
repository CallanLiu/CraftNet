namespace CraftNet.Services;

/// <summary>
/// Actor消息
/// </summary>
public class ActorMessage
{
    /// <summary>
    /// 目标ActorId
    /// </summary>
    public ActorId ActorId { get; }

    /// <summary>
    /// 消息类型
    /// </summary>
    public MessageType Type { get; }

    /// <summary>
    /// 
    /// </summary>
    public ushort Opcode { get; }

    /// <summary>
    /// 关联Id
    /// </summary>
    public uint RpcId { get; }

    public IMessageBase Body { get; }

    /// <summary>
    /// 消息方向
    /// </summary>
    public int Direction { get; }

    /// <summary>
    /// 来源PId
    /// </summary>
    public ushort SenderPId { get; internal set; }

    internal IRpcReply RpcReply { get; set; }

    internal IResponseCompletionSource<IResponse> Tcs { get; set; }

    internal object Target { get; set; }

    /// <summary>
    /// 是否来自对象池
    ///     1. 远程消息，在发送完成后回到池中
    ///     2. 本地消息，在执行分发后回到池中
    /// </summary>
    internal bool IsFromPool { get; set; } = false;

    public ActorMessage(MessageType type, ActorId actorId, ushort opcode, IMessageBase body, uint rpcId,
        IResponseCompletionSource<IResponse> tcs = null, int direction = 0)
    {
        this.Type      = type;
        this.ActorId   = actorId;
        this.Opcode    = opcode;
        this.Body      = body;
        this.RpcId     = rpcId;
        this.Tcs       = tcs;
        this.Direction = direction;
    }

    public static ActorMessage CreateResponse(ActorId actorId, ushort opcode, uint rpcId, IResponse body,
        int direction = 0)
    {
        return new ActorMessage(MessageType.Response, actorId, opcode, body, rpcId, null, direction);
    }

    public static ActorMessage CreateRequest(ActorId actorId, ushort opcode, uint rpcId, IMessageBase body,
        IResponseCompletionSource<IResponse> tcs, int direction = 0)
    {
        return new ActorMessage(MessageType.Request, actorId, opcode, body, rpcId, tcs, direction);
    }

    /// <summary>
    /// 创建一个MessageType.Message类型的Actor消息
    /// </summary>
    /// <param name="actorId"></param>
    /// <param name="opcode"></param>
    /// <param name="body"></param>
    /// <param name="direction">消息方向</param>
    public static ActorMessage CreateMessage(ActorId actorId, ushort opcode, IMessageBase body, int direction = 0)
    {
        return new ActorMessage(MessageType.Message, actorId, opcode, body, 0, null, direction);
    }
}