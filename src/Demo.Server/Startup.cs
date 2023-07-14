using System.Net;
using Demo.Network;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.DependencyInjection;
using CraftNet;
using CraftNet.Services;

namespace Demo;

public static class Startup
{
    public static void ConfigureBuilder(this WebApplicationBuilder builder, StartConfig startConfig)
    {
        builder.WebHost.UseKestrel(options => { Bind(options, startConfig); });

        bool enableLogin = false;
        foreach (var appConfig in startConfig.Current.AppConfigs)
        {
            if (appConfig.Type is AppType.Login && !enableLogin)
            {
                enableLogin = true;

                // 给login配置一下
                builder.Services.AddSingleton<IWebSocketListener, WebSocketService>();
                IMvcBuilder mvcBuilder = builder.Services.AddControllers();
                builder.Services.AddSingleton<DynamicActionDescriptorChangeProvider>();
                builder.Services.AddSingleton<IActionDescriptorChangeProvider>(services =>
                    services.GetService<DynamicActionDescriptorChangeProvider>());
            }
        }
    }

    public static void Configure(this WebApplication webApplication, StartConfig startConfig)
    {
        IServiceProvider services = webApplication.Services;
        ILocalPId        localPId = services.GetService<ILocalPId>();
        services.GetService<IMessageDescCollection>().Add(typeof(AppList).Assembly);

        // 注册其它进程的IP
        ISenderManager senderManager = services.GetService<ISenderManager>();
        foreach (var processConfig in startConfig.Processes)
        {
            senderManager.AddRemote(processConfig.Id, IPEndPoint.Parse(processConfig.EndPoint));
        }

        bool enableGate  = false;
        bool enableLogin = false;
        foreach (var appConfig in startConfig.Current.AppConfigs)
        {
            // 创建App
            AppBuilder appBuilder = new AppBuilder(services);
            appBuilder.AddPlugin(PluginNames.Demo);
            appBuilder.AddSystem<IEventSystem, EventSystem>();
            appBuilder.AddSystem<IActorSystem, ActorSystem>();
            appBuilder.AddSystem<ITimerSystem, TimerSystem>();
            appBuilder.AddComponent(appConfig);

            // 网关
            if (appConfig.Type is AppType.Gate && !enableGate)
            {
                enableGate = true; // 只用配置一次
                webApplication.UseWebSockets();
                webApplication.Map("/gate", services.GetService<IWebSocketListener>().Accept);
            }
            else if (appConfig.Type is AppType.Login && !enableLogin)
            {
                enableLogin = true;

                webApplication.UseAuthentication();
                webApplication.MapControllers();

                appBuilder.AddSystem<IAspNetCoreReloadSystem, AspNetCoreReloadSystem>();

                IActorService actorService = services.GetService<IActorService>();

                // Http需要统一处理器。
                // webApplication.MapPost("/login", async httpContext =>
                // {
                //     var reader = httpContext.Request.BodyReader;
                //     if (!reader.TryRead(out ReadResult readResult))
                //     {
                //         httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                //         return;
                //     }
                //
                //     var buffer = readResult.Buffer;
                //     if (!NetPacket.TryParse(ref buffer, out NetPacket packet))
                //     {
                //         httpContext.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                //         return;
                //     }
                //
                //     // 使用请求
                //     var  loginAppList = AppList.Ins[appConfig.Type];
                //     IApp app          = loginAppList.Rand();
                //
                //     var tcs = ResponseCompletionSourcePool.Get<IResponse>();
                //     ActorMessage actorMessage = new ActorMessage(packet.MsgType, app.Id, packet.Opcode,
                //         (IMessageBase)packet.Body, packet.RpcId, tcs);
                //
                //     actorService.Post(actorMessage);
                //     IResponse response = await tcs.AsValueTask();
                //
                //     // 回给客户端
                //     NetPacket.Send(httpContext.Response.BodyWriter,
                //         new WaitSendPacket(response.GetOpcode(), actorMessage.RpcId, response));
                // });
            }

            var app = appBuilder.Build(new AppId(localPId.Value, appConfig.Index), appConfig.Name);
            AppList.Ins.Add(appConfig.Type, app);
        }
    }

    static void Bind(KestrelServerOptions options, StartConfig startConfig)
    {
        ProcessConfig processConfig = startConfig.Current;
        if (string.IsNullOrEmpty(processConfig.Urls))
            return;

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
}