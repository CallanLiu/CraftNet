namespace CraftNet;

public partial class App : IApp, IComponentCollection
{
    private readonly List<IPluginAssemblyContext> _plugins;

    // 服务
    public readonly IServiceProvider Services;

    public AppId Id { get; }

    public string Name { get; }

    public uint Version { get; private set; }
    public IScheduler Scheduler { get; }

    public bool IsFirstLoad { get; private set; }

    // public Setup Setup { get; }

    private readonly List<Action<App>> _loadConfigures;
    private readonly List<Action<App>> _initConfigures;

    private readonly IComponentCollection _components = new ComponentCollection();

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="data"></param>
    internal App(AppId id, AppCreateData data)
    {
        this.IsFirstLoad = true;
        this.Name        = data.Name;
        Id               = id;
        Services         = data.Services;
        _plugins         = data.Plugins;
        _loadConfigures  = data.LoadConfigures;
        _initConfigures  = data.InitConfigures;

        // 默认使用单线程调度器
        this.Scheduler = data.Scheduler ?? new SingleThreadScheduler(this);
        this.Scheduler.Execute(OnLoad);
    }

    private void OnLoad()
    {
        ++this.Version;
        try
        {
            // 清理
            _groups.Clear();
            _systems = null;

            // 每次都要执行
            if (_loadConfigures != null)
            {
                if (this.IsFirstLoad)
                {
                    foreach (var a in _initConfigures)
                        a(this);
                }

                foreach (var a in _loadConfigures)
                    a(this);
            }

            // 重新载入插件
            foreach (var ctx in _plugins)
            {
                if (this.IsFirstLoad)
                {
                    ctx.OnAssemblyReloadEvent += () => this.Scheduler.Execute(OnLoad);
                }

                ctx.InvokeEntryPoint(this);
            }
        }
        finally
        {
            this.IsFirstLoad = false;
        }
    }

    public override string ToString()
    {
        return $"{Name}_{Id}";
    }


    public T GetComponent<T>() => _components.GetComponent<T>();

    public T AddComponent<T>() where T : new() => _components.AddComponent<T>();

    public T AddComponent<T>(T ins) => _components.AddComponent(ins);

    public bool RemoveComponent<T>() => _components.RemoveComponent<T>();
}