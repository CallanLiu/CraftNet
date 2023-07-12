using Serilog;
using CraftNet;

namespace Demo;

/// <summary>
/// 插件入口点
/// </summary>
public class Main
{
    public static void OnLoad(App app)
    {
        IMessageSystem messageSystem = app.GetSystem<IMessageSystem>();
        if (app.IsFirstLoad)
        {
            messageSystem.CreateActor(app, true);
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