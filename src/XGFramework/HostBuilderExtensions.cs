using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XGFramework.Services;

namespace XGFramework;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseXGServer(this IHostBuilder self)
    {
        self.ConfigureServices(services =>
        {
            services.AddSingleton<IActorService, ActorService>();
            services.AddSingleton<ISenderManager, SenderManager>();
            services.AddSingleton<IMessageSerializer, MemoryPackMessageSerializer>();
            services.AddSocketConnectionFactory();
        });
        return self;
    }
}