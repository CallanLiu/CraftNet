using XGFramework;

namespace Demo;

public class GateSystem : IGateSystem
{
    private readonly App _app;

    public GateSystem(App app)
    {
        _app = app;

        // 注册相关处理器
        IMessageSystem messageSystem = _app.GetSystem<IMessageSystem>();
        messageSystem.RegisterHandler<PingHandler>();
        messageSystem.RegisterHandler<Login2G_GetTokenHandler>();
    }
}