namespace XGFramework;

public partial class App : IApp
{
    private readonly List<IPluginAssemblyContext> _plugins;

    // 服务
    public readonly IServiceProvider Services;

    public AppId Id { get; }

    public string Name { get; }

    public uint Version { get; private set; }
    public IAppScheduler Scheduler { get; }

    public bool IsFirstLoad => Version == 1;

    // public Setup Setup { get; }

    private readonly List<Action<App>> _configureCallbacks;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="id"></param>
    /// <param name="services"></param>
    /// <param name="plugins"></param>
    /// <param name="configures"></param>
    /// <param name="name"></param>
    /// <param name="scheduler">指定调度器</param>
    internal App(AppId id, IServiceProvider services, List<IPluginAssemblyContext> plugins,
        List<Action<App>> configures,
        string name, IAppScheduler scheduler = null)
    {
        this.Name           = name;
        Id                  = id;
        Services            = services;
        _plugins            = plugins;
        _configureCallbacks = configures;
        // Setup               = new Setup(this);

        // 默认使用单线程调度器
        this.Scheduler = scheduler ?? new SingleThreadScheduler(this);
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

            // Setup.Enter(this.Version);

            // 每次都要执行
            if (_configureCallbacks != null)
            {
                foreach (var a in _configureCallbacks)
                    a(this);
            }

            // 重新载入插件
            foreach (var ctx in _plugins)
            {
                if (this.Version == 1)
                    ctx.OnAssemblyReloadEvent += () => this.Scheduler.Execute(OnLoad);

                ctx.InvokeEntryPoint(this);
            }
        }
        finally
        {
            // Setup.Exit();
        }
    }

    public override string ToString()
    {
        return $"{Name}_{Id}";
    }

    private readonly IStateCollection _states = new StateCollection();

    public T GetState<T>() => _states.GetState<T>();

    public T AddState<T>() where T : new() => _states.AddState<T>();

    public T AddState<T>(T ins) => _states.AddState(ins);

    public bool RemoveState<T>() => _states.RemoveState<T>();
}