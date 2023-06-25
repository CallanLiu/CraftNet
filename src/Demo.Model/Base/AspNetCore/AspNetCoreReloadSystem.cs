using Microsoft.AspNetCore.Mvc.ApplicationParts;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using XGFramework;

namespace Demo;

public class AspNetCoreReloadSystem : IAspNetCoreReloadSystem
{
    public AspNetCoreReloadSystem(App app)
    {
        IPluginAssemblyService pluginAssemblyService = app.Services.GetService<IPluginAssemblyService>();
        IPluginAssemblyContext assemblyContext       = pluginAssemblyService.Get("Demo");

        // 载入可重载的程序集
        ApplicationPartManager partManager = app.Services.GetService<ApplicationPartManager>();
        partManager.ApplicationParts.Clear();
        partManager.ApplicationParts.Add(new AssemblyPart(assemblyContext.HotfixAssembly));

        // 每次程序集被重载了，就通知一下AspNetCore重新加载一下Action
        app.Services.GetService<DynamicActionDescriptorChangeProvider>().NotifyChange();
        
        Log.Debug("载入动态Controller程序集: {Name}", assemblyContext.HotfixAssembly.GetName().Name);
    }
}