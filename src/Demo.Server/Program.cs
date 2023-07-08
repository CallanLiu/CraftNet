// See https://aka.ms/new-console-template for more information

using System.Net;
using Demo;
using Demo.Network;
using XGFramework;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

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
builder.Services.AddSingleton<IWebSocketListener, WebSocketService>();
builder.WebHost.UseXGServer(pid, xg =>
{
    xg.AddPlugin("Demo");
    xg.EndPoint = IPEndPoint.Parse(startConfig.Current.EndPoint);
});

Startup.ConfigureApps(builder, startConfig);

var app = builder.Build();

Startup.CreateApps(app, startConfig);

// 3.运行host
await app.RunAsync();