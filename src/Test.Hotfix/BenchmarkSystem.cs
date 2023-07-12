using System.Diagnostics;
using Serilog;
using CraftNet;
using CraftNet.Services;

namespace Test.Hotfix;

/// <summary>
/// 系统只有逻辑与只读字段的服务。
/// </summary>
public class BenchmarkSystem : IBenchmarkSystem
{
    private static int _callCount = 0;       // 当前已调用次数
    public const   int LogCount   = 1000000; // 100w
    private const  int ClientNum  = 2000;    // 模拟客户端的数量

    private readonly App            _app;
    private readonly TestSystemComp _comp;

    public BenchmarkSystem(App app)
    {
        _app = app;

        if (_app.IsFirstLoad)
        {
            app.AddComponent<TestSystemComp>();
        }

        _comp = app.GetComponent<TestSystemComp>();

        // 让测试死循环退出来，不然热重载后，上个程序集无法释放。
        _comp.CancellationTokenSource.Cancel();
        _comp.CancellationTokenSource.Dispose();
        _comp.CancellationTokenSource = new CancellationTokenSource();

        // 执行测试
        if (app.Id.PId == 2) // client
        {
            BenchmarkTest();
        }
    }

    private async void BenchmarkTest()
    {
        try
        {
            List<Task> tasks = new List<Task>();

            //并行模拟多个客户端
            for (int i = 0; i < ClientNum; i++)
            {
                tasks.Add(OneClientBenchmarkTest());
            }

            await Task.WhenAll(tasks);
            Log.Debug("BenchmarkTest完成...");
        }
        catch (Exception e)
        {
            Log.Error(e, "测试Ping异常:");
        }
    }

    private async Task OneClientBenchmarkTest()
    {
        // 指定测试服的id
        AppId                   srvId         = new AppId(1, 0);
        IMessageSystem          messageSystem = _app.GetSystem<IMessageSystem>();
        Stopwatch               stopwatch     = _comp.Stopwatch;
        CancellationTokenSource cts           = _comp.CancellationTokenSource;
        while (cts.IsCancellationRequested == false)
        {
            IResponse response = await messageSystem.Call(srvId, new PingAppReq());
            if (Interlocked.Increment(ref _callCount) % LogCount == 0)
            {
                stopwatch.Stop();
                Console.WriteLine($"call ping: {LogCount}次，耗时{stopwatch.ElapsedMilliseconds}ms");
                stopwatch.Restart();
            }
        }
    }
}