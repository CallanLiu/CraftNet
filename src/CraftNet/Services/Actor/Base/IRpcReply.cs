namespace CraftNet.Services;

public interface IRpcReply
{
    void OnRpcReply(IResponse resp, ActorMessage context);
}