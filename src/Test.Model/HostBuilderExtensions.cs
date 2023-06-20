using Test;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using XGFramework;
using XGFramework.Services;

namespace Test;

public static class HostBuilderExtensions
{
    public static IHostBuilder UseXGServer(this IHostBuilder self)
    {
        self.ConfigureServices(services =>
        {
            services.AddSingleton<IActorService, ActorService>();
            services.AddSingleton<ISenderManager, SenderManager>();
            services.AddSocketConnectionFactory();
        });
        return self;
    }
}