namespace CraftNet.Services;

/// <summary>
/// 扩展调用
/// </summary>
public static class ActorServiceExtensions
{
    private static uint _rpcIds = 0;

    public static void Send<T>(this IActorService self, ActorId actorId, T msg) where T : IMessage, IMessageMeta
    {
        self.Post(actorId, MessageType.Message, T.Opcode, 0, msg);
    }

    public static async ValueTask<TResp> Call<TResp>(this IActorService self, ActorId actorId, IRequest req)
        where TResp : IResponse, IMessageMeta
    {
        ResponseCompletionSource<IResponse> tcs = ResponseCompletionSourcePool.Get<IResponse>();
        self.Post(actorId, MessageType.Request, req.GetOpcode(), Interlocked.Increment(ref _rpcIds), req, tcs);
        IResponse response = await tcs.AsValueTask().ConfigureAwait(false);
        return (TResp)response;
    }
}