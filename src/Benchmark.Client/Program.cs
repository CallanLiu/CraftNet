// See https://aka.ms/new-console-template for more information

using System.Net;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Test;
using XGFramework;
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
    builder.Services.AddSingleton<IMessageSerializer, ProtoMessageSerializer>();
    builder.Services.AddHostedService<TestHostedService>();

    builder.WebHost.UseXGServer(2, xg =>
    {
        xg.AddPlugin("Test");
        xg.EndPoint = new IPEndPoint(IPAddress.Loopback, 20001);
    });

    var app = builder.Build();

    // 3.运行host
    await app.RunAsync();
}
catch (Exception e)
{
    Log.Fatal(e, "");
}

Console.ReadKey();