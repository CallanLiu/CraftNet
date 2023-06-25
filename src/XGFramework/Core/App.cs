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

    public bool IsFirstLoad { get; private set; }

    // public Setup Setup { get; }

    private readonly List<Action<App>> _loadConfigures;
    private readonly List<Action<App>> _initConfigures;

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

    private readonly IStateCollection _states = new StateCollection();

    public T GetState<T>() => _states.GetState<T>();

    public T AddState<T>() where T : new() => _states.AddState<T>();

    public T AddState<T>(T ins) => _states.AddState(ins);

    public bool RemoveState<T>() => _states.RemoveState<T>();
}