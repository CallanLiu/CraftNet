namespace XGFramework;

public class Setup : ISetup
{
    private uint _version;

    public App App { get; }

    public bool IsFirst => _version == 1;

    public bool IsEnter { get; private set; } = false;

    private Dictionary<Type, ISystemSetup> _systemSetups;

    public Setup(App app)
    {
        App = app;
    }

    public void Enter(uint version)
    {
        this._version = version;
        IsEnter       = true;
        _systemSetups = new Dictionary<Type, ISystemSetup>();
    }

    public void Exit()
    {
        IsEnter       = false;
        _systemSetups = null;
    }

    public void RegisterSetup<T>(T self) where T : ISystemSetup
    {
        _systemSetups.Add(typeof(T), self);
    }

    public T GetSetup<T>() where T : ISystemSetup
    {
        _systemSetups.TryGetValue(typeof(T), out var obj);
        return (T)obj;
    }

    public void CheckThrowException()
    {
        if (!IsEnter)
        {
            throw new Exception($"Setup状态错误: IsEnter=false");
        }
    }
}