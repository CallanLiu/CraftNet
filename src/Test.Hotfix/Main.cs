using System.Diagnostics;
using Serilog;
using XGFramework;
using XGFramework.Services;

namespace Test.Hotfix;

public static class SystemGroup
{
    public const uint Player = 1;
    public const uint Home   = 2;
}

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

        messageSystem.RegisterHandler<TestWsMessageHandler>();
        messageSystem.RegisterHandler<TestMessageHandler>();
        messageSystem.RegisterHandler<PingHandler>();
        messageSystem.RegisterHandler<PingAppReqHandler>();
        messageSystem.RegisterFilter<AgentMessageFilter>();

        if (app.Id.PId == 1) // server
        {
        }
        else if (app.Id.PId == 2) // server
        {
            app.AddSystem<ITestSystem, TestSystem>();
        }
    }
}