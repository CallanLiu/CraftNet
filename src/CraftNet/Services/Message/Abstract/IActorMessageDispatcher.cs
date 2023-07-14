namespace CraftNet.Services;

public interface IRpcReply
{
    void OnRpcReply(IResponse resp, ActorMessage context);
}

/// <summary>
/// Actor分发接口
/// </summary>
public interface IActorMessageDispatcher
{
    ValueTask Dispatch(int? filterId, ActorMessage actorMessage);
}