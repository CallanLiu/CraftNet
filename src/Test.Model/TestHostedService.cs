using System.Net;
using Microsoft.Extensions.Hosting;
using Serilog;
using XGFramework;
using XGFramework.Services;

namespace Test;

public class TestHostedService : BackgroundService
{
    private readonly IServiceProvider _services;
    private readonly ISenderManager   _senderManager;
    private readonly ILocalPId        _localPId;

    public TestHostedService(IServiceProvider services, ISenderManager senderManager, ILocalPId localPId)
    {
        _services      = services;
        _senderManager = senderManager;
        _localPId      = localPId;
    }

    protected override Task ExecuteAsync(CancellationToken stoppingToken)
    {
        _senderManager.AddServer(1, IPEndPoint.Parse("127.0.0.1:20000"));
        _senderManager.AddServer(2, IPEndPoint.Parse("127.0.0.1:20001"));

        // 注册网络消息
        OpcodeCollection.Ins.Add<TestWsMessage>();
        OpcodeCollection.Ins.Add<PingReq>();
        OpcodeCollection.Ins.Add<PingResp>();
        OpcodeCollection.Ins.Add<PingAppReq>();
        OpcodeCollection.Ins.Add<PingAppResp>();

        // 创建网关app
        AppBuilder gateBuilder = new AppBuilder(_services);
        gateBuilder.AddPlugin("Test");
        gateBuilder.AddSystem<IEventSystem, EventSystem>();
        gateBuilder.AddSystem<IMessageSystem, MessageSystem>();
        IApp gateApp = gateBuilder.Build(new AppId(_localPId.Value, 0), "gate");
        Log.Debug("创建App: {Info}", gateApp.ToString());

        // 用处理客户端的网络消息
        TestWsListener.Apps[0] = gateApp;

        return Task.CompletedTask;
    }
}