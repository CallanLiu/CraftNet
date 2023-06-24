using Test.Hotfix;
using XGFramework;

namespace Test;

/// <summary>
/// 插件入口点
/// </summary>
public static class Main
{
    public static void OnLoad(App app)
    {
        IMessageSystem messageSystem = app.GetSystem<IMessageSystem>();

        if (app.IsFirstLoad)
        {
            // 让app成为actor，使其可以接收消息。
            messageSystem.CreateActor(app, true);
        }

        messageSystem.RegisterHandler<PingAppReqHandler>();

        if (app.Id.PId == 1) // server
        {
        }
        else if (app.Id.PId == 2) // client
        {
            app.AddSystem<IBenchmarkSystem, BenchmarkSystem>();
        }
    }
}