using CraftNet;
using Demo.Services.Network;
using Serilog;

namespace Demo.Events;

public class NetworkConnectedFuncImpl : FuncImpl<NetworkConnectedFunc, Session>
{
    protected override Session On(in NetworkConnectedFunc e)
    {
        IActorSystem actorSystem = this.App.GetSystem<IActorSystem>();
        Session      session     = new Session(e.RemoteEndPoint, e.SendToClientAction);
        session.ActorId = actorSystem.CreateActor<SessionMessageFilter>(session);
        session.Mailbox = actorSystem.GetActorMailbox(session.ActorId);
        Log.Debug("连接成功: {Ws}", e.RemoteEndPoint);
        return session;
    }
}