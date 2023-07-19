using Serilog;
using CraftNet;
using Microsoft.Extensions.DependencyInjection;

namespace Demo;

/// <summary>
/// 插件入口点
/// </summary>
public class Main
{
    public static void OnLoad(App app)
    {
        IActorSystem actorSystem = app.GetSystem<IActorSystem>();
        if (app.IsFirstLoad)
        {
            // 让每个app都成为一个actor
            actorSystem.CreateActor(app, true);
        }

        AppConfig appConfig = app.GetComponent<AppConfig>();
        switch (appConfig.Type)
        {
            case AppType.Gate:
            {
                app.AddSystem<IGateSystem, GateSystem>();
                break;
            }
        }

        Log.Debug("Load APP：{Name}, {Type}", app.Name, appConfig.Type);
    }
}