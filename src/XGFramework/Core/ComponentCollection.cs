namespace XGFramework;

public class ComponentCollection : IComponentCollection
{
    private Dictionary<int, IComp> components = new Dictionary<int, IComp>();

    public T GetComp<T>() where T : IComp
    {
        int id = IComponentCollection.Id<T>.Value;
        components.TryGetValue(id, out var obj);
        return (T)obj;
    }

    public T AddComp<T>() where T : IComp, new()
    {
        int id = IComponentCollection.Id<T>.Value;
        if (components.ContainsKey(id))
            throw new ArgumentException($"组件已存在: id={id} type={typeof(T).Name}");

        T t = new T();
        components.Add(id, t);
        return t;
    }

    public bool RemoveComp<T>() where T : IComp
    {
        int id = IComponentCollection.Id<T>.Value;
        return components.Remove(id);
    }
}