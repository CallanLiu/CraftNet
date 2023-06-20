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
            TestPing(app);
        }

        // IEventSystem eventSystem = app.GetSystem<IEventSystem>();
        // eventSystem.RegisterListener<OnWebSocketConnectedListener>();
        //
        // app.AddSystem<ITestSystem, TestSystem>();
        // app.GetSystem<ITestSystem>().Test();
    }

    private static int       callCount = 0;
    private const  int       logCount  = 1000000; // 100w
    const          int       clientNum = 2000;    // 并行
    private static Stopwatch stopwatch = Stopwatch.StartNew();

    private static async void TestPing(App app)
    {
        try
        {
            AppId srvId = new AppId(1, 0);

            var msgSys = app.GetSystem<IMessageSystem>();

            List<Task> tasks = new List<Task>();

            //并行模拟多个客户端
            for (int i = 0; i < clientNum; i++)
            {
                tasks.Add(PingReqTest(srvId, msgSys));
            }

            await Task.WhenAll(tasks);

            static async Task PingReqTest(ActorId target, IMessageSystem messageSystem)
            {
                while (true)
                {
                    IResponse response = await messageSystem.Call(target, new PingAppReq());
                    if (Interlocked.Increment(ref callCount) % logCount == 0)
                    {
                        stopwatch.Stop();
                        Console.WriteLine($"call ping: {logCount}次，耗时{stopwatch.ElapsedMilliseconds}ms");
                        stopwatch.Restart();
                    }
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "测试Ping异常:");
        }
    }
}