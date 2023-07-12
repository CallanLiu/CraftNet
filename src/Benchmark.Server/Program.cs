// See https://aka.ms/new-console-template for more information

using System.Net;
using CraftNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Test;
using CraftNet.Services;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Debug()
    .WriteTo.Console()
    .CreateLogger();

try
{
    // 1.host初始化
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog();

    builder.Services.AddSingleton<IMessageSerializer, ProtoMessageSerializer>();
    builder.Services.AddHostedService<TestHostedService>();
    // builder.Services.AddSingleton<IWebSocketListener, TestWsListener>();

    builder.WebHost.UseCraftNet(1, xg =>
    {
        xg.AddPlugin("Test");
        xg.EndPoint = new IPEndPoint(IPAddress.Loopback, 20000);
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