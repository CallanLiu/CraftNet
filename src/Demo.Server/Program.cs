// See https://aka.ms/new-console-template for more information

using System.Net;
using Demo;
using Demo.Network;
using XGFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using XGFramework.ProtoGen;
using XGFramework.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

// 1.host初始化
var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// 2.载入启动配置
var    cfg             = builder.Configuration;
ushort pid             = cfg.GetValue<ushort>("pid");
string startConfigName = cfg.GetValue<string>("cfg");

StartConfig startConfig = StartConfig.Load(pid, startConfigName);
if (startConfig is null)
{
    Log.Error("启动配置不存在: {Path}", startConfigName);
    return;
}

builder.Services.AddSingleton(startConfig);
builder.Services.AddSingleton<ILocalPId>(new LocalPId { Value = pid });
builder.Services.AddSingleton<IWebSocketListener, WebSocketService>();
builder.Host.ConfigPluginAssembly(list => { list.Add("Demo"); });
builder.WebHost.UseKestrel(options => { Bind(options, startConfig); });
builder.Host.UseXGServer();

var app = builder.Build();

CreateApps(app, startConfig);

// 3.运行host
await app.RunAsync();

static void Bind(KestrelServerOptions options, StartConfig startConfig)
{
    ProcessConfig processConfig = startConfig.Current;
    if (string.IsNullOrEmpty(processConfig.Urls))
        return;

    // 内部
    options.Listen(IPEndPoint.Parse(processConfig.EndPoint),
        l => { l.UseConnectionHandler<ServerConnectionHandler>(); });

    // 外部http与ws
    string[] urls = processConfig.Urls.Split(";");
    foreach (var url in urls)
    {
        BindingAddress address = BindingAddress.Parse(url);
        if (!address.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
            continue;

        if (string.Equals(address.Host, "localhost", StringComparison.OrdinalIgnoreCase))
        {
            // "localhost" for both IPv4 and IPv6 can't be represented as an IPEndPoint.
            // options = new LocalhostListenOptions(parsedAddress.Port);
            options.ListenLocalhost(address.Port);
        }
        else if (IPAddress.TryParse(address.Host, out IPAddress ip))
        {
            options.Listen(new IPEndPoint(ip, address.Port));
        }
        else
        {
            options.ListenAnyIP(address.Port);
        }
    }
}

static void CreateApps(WebApplication webApplication, StartConfig startConfig)
{
    IServiceProvider services = webApplication.Services;
    ILocalPId        localPId = services.GetService<ILocalPId>();

    // 注册其它进程的IP
    ISenderManager senderManager = services.GetService<ISenderManager>();
    foreach (var processConfig in startConfig.Processes)
    {
        senderManager.AddRemote(processConfig.Id, IPEndPoint.Parse(processConfig.EndPoint));
    }

    OpcodeCollection.Ins.Add(typeof(C2G_PingReq).Assembly);

    bool enableWs = false;
    foreach (var appConfig in startConfig.Current.AppConfigs)
    {
        // 有网关需要配置一下ws
        if ("gate".Equals(appConfig.Type, StringComparison.OrdinalIgnoreCase))
            enableWs = true;

        // 创建App
        AppBuilder appBuilder = new AppBuilder(services);
        appBuilder.AddPlugin("Demo");
        appBuilder.AddSystem<IEventSystem, EventSystem>();
        appBuilder.AddSystem<IMessageSystem, MessageSystem>();
        appBuilder.AddState(appConfig);
        var app = appBuilder.Build(new AppId(localPId.Value, appConfig.Index), appConfig.Name);

        // 网关
        if ("gate".Equals(appConfig.Type, StringComparison.OrdinalIgnoreCase))
        {
            WebSocketService.Apps.Add(app);
        }
    }

    if (enableWs)
    {
        webApplication.UseWebSockets();
        webApplication.Map("/ws", services.GetService<IWebSocketListener>().Accept);
    }
}