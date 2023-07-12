using Microsoft.Extensions.DependencyInjection;

namespace CraftNet;

public class AppBuilder
{
    private readonly IServiceProvider       _services;
    private readonly IPluginAssemblyService _pluginAssemblyService;

    private readonly List<IPluginAssemblyContext> _plugins = new();

    // 每次Load都会调用
    private readonly List<Action<App>> _appLoadConfigures = new();

    // 只有第一次load才会调用
    private readonly List<Action<App>> _appInitConfigures = new();

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
        _appLoadConfigures.Add(app => { app.AddSystem<T, TImpl>(); });
    }

    public void AddComponent<T>(T ins)
    {
        _appInitConfigures.Add(app => { app.AddComponent(ins); });
    }

    public IApp Build(AppId appId = default, string name = null)
    {
        App app = new App(appId, new AppCreateData(name, _services, _plugins, _appInitConfigures, _appLoadConfigures));
        return app;
    }
}