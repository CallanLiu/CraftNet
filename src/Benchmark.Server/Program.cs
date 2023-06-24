// See https://aka.ms/new-console-template for more information

using XGFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Test;
using XGFramework.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

try
{
    // 1.host初始化
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();
    builder.Host.ConfigPluginAssembly(list => { list.Add("Test"); });
    builder.Host.UseXGServer();
    builder.Services.AddSingleton<IMessageSerializer, ProtoMessageSerializer>();
    builder.Services.AddSingleton<ILocalPId>(new LocalPId { Value = 1 });
    builder.Services.AddHostedService<TestHostedService>();
    // builder.Services.AddSingleton<IWebSocketListener, TestWsListener>();

    builder.WebHost.UseKestrel(options =>
    {
        options.ListenLocalhost(20000, l => { l.UseConnectionHandler<ServerConnectionHandler>(); });
    });

    var app = builder.Build();

    // app.UseWebSockets();
    // app.Map("/ws", app.Services.GetService<IWebSocketListener>().Accept);

    // 3.运行host
    await app.RunAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "");
}

Console.ReadKey();