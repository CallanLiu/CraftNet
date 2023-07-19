// See https://aka.ms/new-console-template for more information

using System.Net;
using Demo;
using Demo.Network;
using CraftNet;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using ProtoBuf.Meta;
using Serilog;
using Serilog.Events;
using Tomlyn.Extensions.Configuration;

Log.Logger = new LoggerConfiguration()
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .CreateBootstrapLogger();

try
{
    // 1.host初始化
    var builder = WebApplication.CreateBuilder(args);
    builder.Host.UseSerilog((context, services, configuration) => configuration
        .ReadFrom.Configuration(context.Configuration)
        .ReadFrom.Services(services));

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
    builder.WebHost.UseCraftNet(pid, xg =>
    {
        xg.AddPlugin(PluginNames.Demo);
        xg.EndPoint = IPEndPoint.Parse(startConfig.Current.EndPoint);
    });

    builder.ConfigureBuilder(startConfig);
    var app = builder.Build();
    app.Configure(startConfig);

    // 3.运行host
    await app.RunAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "");
}