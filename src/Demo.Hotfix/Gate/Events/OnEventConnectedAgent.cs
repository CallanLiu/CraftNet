using CraftNet;
using CraftNet.Services;

namespace Demo.Events;

public class OnEventConnectedAgent : EventInvokable<EventSessionConnected, ActorId>
{
    public override ActorId On(EventSessionConnected e)
    {
        IMessageSystem messageSystem = this.App.GetSystem<IMessageSystem>();
        Session        session       = new Session(e.RemoteEndPoint, e.SendToClientAction);
        session.ActorId = messageSystem.CreateActor<SessionMessageFilter>(session);
        return session.ActorId;
    }
}