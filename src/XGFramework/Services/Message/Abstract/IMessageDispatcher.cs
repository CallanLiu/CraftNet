namespace XGFramework.Services;

public interface IRpcReply
{
    void Invoke(IResponse resp, ActorMessage context);
}



public interface IMessageDispatcher
{
    ValueTask Dispatch(int? filterId, ActorMessage actorMessage);
}