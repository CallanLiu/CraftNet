namespace CraftNet.Services;

/// <summary>
/// 扩展调用
/// </summary>
public static class ActorServiceExtensions
{
    private static uint _rpcIds = 0;

    public static void Send<T>(this IActorService self, ActorId actorId, T msg) where T : IMessage, IMessageMeta
    {
        self.Post(ActorMessage.CreateMessage(actorId, T.Opcode, msg, 0));
    }

    public static async ValueTask<TResp> Call<TResp>(this IActorService self, ActorId actorId, IRequest req)
        where TResp : IResponse, IMessageMeta
    {
        // ResponseCompletionSource<IResponse> tcs = ResponseCompletionSourcePool.Get<IResponse>();
        ResponseCompletionSource<IResponse> tcs = new ResponseCompletionSource<IResponse>();
        self.Post(ActorMessage.CreateRequest(actorId, req.GetOpcode(), ++_rpcIds, req, tcs));
        IResponse response = await tcs.AsValueTask().ConfigureAwait(false); // 不用回到之前的上线文中了
        return (TResp)response;
    }
}