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

    public MessageType Type { get; }
    public ushort Opcode { get; }
    public uint RpcId { get; }

    public IMessageBase Body { get; }

    public int Extra { get; }

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
        IResponseCompletionSource<IResponse> tcs = null, int extra = 0)
    {
        this.Type    = type;
        this.ActorId = actorId;
        this.Opcode  = opcode;
        this.Body    = body;
        this.RpcId   = rpcId;
        this.Tcs     = tcs;
        this.Extra   = extra;
    }
}