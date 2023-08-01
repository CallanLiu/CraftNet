using System.Net;
using CraftNet;
using CraftNet.Services;
using Demo.Events;

namespace Demo;

public class GateSrv : IGateSrv
{
    private readonly App         _app;
    private readonly GateSrvComp _gateSrvComp;

    public GateSrv(App app)
    {
        _app = app;

        if (_app.IsFirstLoad)
        {
            GateSrvComp gateSrvComp = app.AddComponent<GateSrvComp>();
            gateSrvComp.Sessions = new Slots<Session>();
        }

        _gateSrvComp = app.GetComponent<GateSrvComp>();

        // 注册相关处理器
        IActorSystem actorSystem = _app.GetSystem<IActorSystem>();
        actorSystem.RegisterFilter<SessionMessageFilter>();
        actorSystem.RegisterHandler<PingHandler>();
        actorSystem.RegisterHandler<Login2G_GetTokenHandler>();
        actorSystem.RegisterHandler<C2G_LoginGateHandler>();

        IEventSystem eventSystem = _app.GetSystem<IEventSystem>();
        eventSystem.RegisterCallback<NetworkConnectedFuncImpl>();
    }

    public Session CreateSession(IPEndPoint remoteEndPoint, SendToClientAction sendToClientAction)
    {
        if (!_gateSrvComp.Sessions.TryAdd(null, out uint handle))
            return null;

        IActorSystem actorSystem = _app.GetSystem<IActorSystem>();
        IActor       actor       = actorSystem.CreateActor<SessionMessageFilter>(ActorType<Session>.Value, handle);
        Session      session     = new Session(handle, actor, remoteEndPoint, sendToClientAction);
        _gateSrvComp.Sessions[handle] = session;
        return session;
    }
}