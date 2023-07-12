namespace CraftNet;

public class ComponentCollection : IComponentCollection
{
    private readonly Dictionary<int, object> _components = new();

    public T GetComponent<T>()
    {
        int id = IComponentCollection.Id<T>.Value;
        _components.TryGetValue(id, out var obj);
        return (T)obj;
    }

    public T AddComponent<T>() where T : new()
    {
        int id = IComponentCollection.Id<T>.Value;
        if (_components.ContainsKey(id))
            throw new ArgumentException($"组件已存在: id={id} type={typeof(T).Name}");

        T t = new T();
        _components.Add(id, t);
        return t;
    }

    public T AddComponent<T>(T ins)
    {
        int id = IComponentCollection.Id<T>.Value;
        if (_components.ContainsKey(id))
            throw new ArgumentException($"组件已存在: id={id} type={typeof(T).Name}");
        _components.Add(id, ins);
        return ins;
    }

    public bool RemoveComponent<T>()
    {
        int id = IComponentCollection.Id<T>.Value;
        return _components.Remove(id);
    }

    public Dictionary<int, object>.ValueCollection GetAllComponent() => _components.Values;
}