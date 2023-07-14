using CraftNet;
using Demo.Services.Network;

namespace Demo.Events;

public class NetworkConnectedFuncImpl : FuncImpl<NetworkConnectedFunc, Session>
{
    protected override Session On(in NetworkConnectedFunc e)
    {
        IActorSystem actorSystem = this.App.GetSystem<IActorSystem>();
        Session      session     = new Session(e.RemoteEndPoint, e.SendToClientAction);
        session.ActorId = actorSystem.CreateActor<SessionMessageFilter>(session);
        session.Mailbox = actorSystem.GetActorMailbox(session.ActorId);
        return session;
    }
}