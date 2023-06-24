namespace XGFramework;

public class StateCollection : IStateCollection
{
    private readonly Dictionary<int, object> _states = new();

    public T GetState<T>()
    {
        int id = IStateCollection.Id<T>.Value;
        _states.TryGetValue(id, out var obj);
        return (T)obj;
    }

    public T AddState<T>() where T : new()
    {
        int id = IStateCollection.Id<T>.Value;
        if (_states.ContainsKey(id))
            throw new ArgumentException($"组件已存在: id={id} type={typeof(T).Name}");

        T t = new T();
        _states.Add(id, t);
        return t;
    }

    public T AddState<T>(T ins)
    {
        int id = IStateCollection.Id<T>.Value;
        if (_states.ContainsKey(id))
            throw new ArgumentException($"组件已存在: id={id} type={typeof(T).Name}");
        _states.Add(id, ins);
        return ins;
    }

    public bool RemoveState<T>()
    {
        int id = IStateCollection.Id<T>.Value;
        return _states.Remove(id);
    }
}