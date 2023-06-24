using Microsoft.Extensions.DependencyInjection;

namespace XGFramework;

public class AppBuilder
{
    private readonly IServiceProvider       _services;
    private readonly IPluginAssemblyService _pluginAssemblyService;

    private readonly List<IPluginAssemblyContext> _plugins       = new();
    private readonly List<Action<App>>            _appConfigures = new();

    public AppBuilder(IServiceProvider services)
    {
        _services              = services;
        _pluginAssemblyService = services.GetService<IPluginAssemblyService>();
    }

    /// <summary>
    /// 添加一个插件
    /// </summary>
    /// <param name="name">插件程序集名称</param>
    /// <param name="entryType">指定入口类型</param>
    public void AddPlugin(string name, string entryType = null)
    {
        IPluginAssemblyContext pluginAssemblyContext = _pluginAssemblyService.Get(name);
        if (pluginAssemblyContext is null)
        {
            throw new ArgumentException($"插件程序集不存在: {name}", nameof(name));
        }

        _plugins.Add(pluginAssemblyContext);
    }

    public void AddSystem<T, TImpl>() where T : ISystemBase
        where TImpl : T
    {
        _appConfigures.Add(app => { app.AddSystem<T, TImpl>(); });
    }

    public void AddState<T>(T ins)
    {
        _appConfigures.Add(app => { app.AddState(ins); });
    }

    public IApp Build(AppId appId = default, string name = null)
    {
        App app = new App(appId, _services, _plugins, _appConfigures, name);
        return app;
    }
}