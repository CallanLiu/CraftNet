using Test;
using Serilog;
using XGFramework;
using XGFramework.Services;

namespace Test.Hotfix;

/// <summary>
/// 系统只有逻辑与只读字段的服务。
/// </summary>
public class TestSystem : ITestSystem
{
    private readonly App _app;

    public TestSystem(App app)
    {
        _app = app;

        Console.WriteLine($"创建TestSystem....");

        TestCall();
    }

    private async void TestCall()
    {
        try
        {
            var msgSys = _app.GetSystem<IMessageSystem>();
            for (int i = 0; i < 10; i++)
            {
                IResponse response = await msgSys.Call(_app.Id, new PingAppReq());
                Console.WriteLine("本地调用: ping 响应了...");
            }

            // 连接服务端
            WebSocketClient client = await WebSocketClient.ConnectAsync("ws://127.0.0.1:5000/ws");
            client.Send(new TestWsMessage { Text = "测试消息..." });
            PingResp resp = (PingResp)await client.Call(new PingReq());
            Console.WriteLine("ping 响应:" + resp.Msg);
            await client.DisposeAsync();
        }
        catch (Exception e)
        {
            Log.Error(e, "");
        }
    }

    public void Test()
    {
        Console.WriteLine("Hello TestSystem...");
    }
}