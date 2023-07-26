using System.Net;
using CraftNet.Services;
using MemoryPack;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace CraftNet;

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
    public static IWebHostBuilder UseCraftNet(this IWebHostBuilder self, ushort pid, Action<IXGServerBuilder> action)
    {
        XGServerBuilder serverBuilder = new XGServerBuilder();
        action?.Invoke(serverBuilder);
        self.ConfigureServices(services =>
        {
            services.TryAddSingleton<ILocalPId>(new LocalPId { Value = pid });
            services.TryAddSingleton<IPluginAssemblyService>(new PluginAssemblyService(serverBuilder.Plugins.ToList()));

            // 消息相关
            services.TryAddSingleton<IMessageDescCollection, MessageDescCollection>();
            services.TryAddSingleton<IMessageSerializerProvider, DefaultMessageSerializerProvider>();
            services.TryAddSingleton<IActorService, ActorService>();
            services.TryAddSingleton<ISenderManager, SenderManager>();

            // 定时器
            services.TryAddSingleton<ITimerService, TimerWheel>();
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