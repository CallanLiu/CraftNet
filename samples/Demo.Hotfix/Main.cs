using Serilog;
using CraftNet;
using CraftNet.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Demo;

/// <summary>
/// 插件入口点
/// </summary>
public class Main
{
    public static void OnLoad(App app)
    {
        AppConfig appConfig = app.GetComponent<AppConfig>();

        IActorSystem actorSystem = app.GetSystem<IActorSystem>();
        if (app.IsFirstLoad)
        {
            // 让每个app都成为一个actor
            actorSystem.CreateActor(appConfig.ActorId.Type, appConfig.ActorId.Key);
        }

        switch (appConfig.Type)
        {
            case AppType.Gate:
            {
                app.AddSystem<IGateSrv, GateSrv>();
                break;
            }
        }

        Log.Debug("Load APP：{Name}, {Type}", app.Name, appConfig.Type);
    }
}