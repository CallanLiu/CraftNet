using Test.Hotfix;
using CraftNet;
using CraftNet.Services;

namespace Test;

/// <summary>
/// 插件入口点
/// </summary>
public static class Main
{
    public static void OnLoad(App app)
    {
        IActorSystem actorSystem = app.GetSystem<IActorSystem>();

        if (app.IsFirstLoad)
        {
            // 让app成为actor，使其可以接收消息。
            actorSystem.CreateActor(ActorType<App>.Value, app.Id.Value);
        }

        actorSystem.RegisterHandler<PingAppReqHandler>();

        if (app.Id.PId == 1) // server
        {
        }
        else if (app.Id.PId == 2) // client
        {
            app.AddSystem<IBenchmarkSystem, BenchmarkSystem>();
        }
    }
}