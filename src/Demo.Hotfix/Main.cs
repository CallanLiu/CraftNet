using Serilog;
using XGFramework;

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

        AppConfig appConfig = app.GetState<AppConfig>();
        switch (appConfig.Type)
        {
            case "Gate":
            {
                app.AddSystem<IGateSystem, GateSystem>();
                break;
            }
        }

        Log.Debug("创建APP：{Name}, {Type}", app.Name, appConfig.Type);
    }
}