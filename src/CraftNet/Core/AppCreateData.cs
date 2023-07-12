namespace CraftNet;

public readonly record struct AppCreateData(
    string Name,
    IServiceProvider Services,
    List<IPluginAssemblyContext> Plugins,
    List<Action<App>> InitConfigures,
    List<Action<App>> LoadConfigures,
    IAppScheduler Scheduler = null);