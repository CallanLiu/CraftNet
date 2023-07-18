using CraftNet;
using Demo.Events;

namespace Demo;

public class GateSystem : IGateSystem
{
    private readonly App _app;

    public GateSystem(App app)
    {
        _app = app;

        // 注册相关处理器
        IActorSystem actorSystem = _app.GetSystem<IActorSystem>();
        actorSystem.RegisterFilter<SessionMessageFilter>();
        actorSystem.RegisterHandler<PingHandler>();
        actorSystem.RegisterHandler<Login2G_GetTokenHandler>();

        IEventSystem eventSystem = _app.GetSystem<IEventSystem>();
        eventSystem.RegisterCallback<NetworkConnectedFuncImpl>();
    }
}