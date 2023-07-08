using System.Net;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using XGFramework.Services;

namespace XGFramework;

public interface IXGServerBuilder
{
    IPEndPoint EndPoint { get; set; }
    void AddPlugin(string name);
}

internal class XGServerBuilder : IXGServerBuilder
{
    public readonly HashSet<string> Plugins = new HashSet<string>();
    public IPEndPoint EndPoint { get; set; }

    public void AddPlugin(string name)
    {
        Plugins.Add(name);
    }
}

public static class HostBuilderExtensions
{
    public static IWebHostBuilder UseXGServer(this IWebHostBuilder self, ushort pid, Action<IXGServerBuilder> action)
    {
        XGServerBuilder serverBuilder = new XGServerBuilder();
        action?.Invoke(serverBuilder);
        self.ConfigureServices(services =>
        {
            services.AddSingleton<IPluginAssemblyService>(new PluginAssemblyService(serverBuilder.Plugins.ToList()));
            services.AddSingleton<IActorService, ActorService>();
            services.AddSingleton<ISenderManager, SenderManager>();
            services.AddSingleton<IMessageSerializer, MemoryPackMessageSerializer>();
            services.AddSingleton<ITimerService, TimerWheel>();
            services.AddSingleton<ILocalPId>(new LocalPId { Value = pid });
            services.AddSocketConnectionFactory();
        });

        if (serverBuilder.EndPoint is not null)
        {
            self.UseKestrel(options =>
            {
                options.Listen(serverBuilder.EndPoint, l => l.UseConnectionHandler<ServerConnectionHandler>());
            });
        }

        return self;
    }
}