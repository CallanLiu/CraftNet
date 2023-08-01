using CraftNet;

namespace Demo.Entities;

public class Entity : GObject, IComponentCollection
{
    private readonly Dictionary<int, object> _components = new();
    private          LinkedList<Entity>      _children;
    private          LinkedListNode<Entity>  _parentNode;
    public App App { get; private set; }

    public Entity Parent
    {
        get => _parentNode?.Value;
        set
        {
            Entity newParent = value;
            if (_parentNode?.Value == newParent)
                return;

            // 从之前的移除掉，并放到新的里面。
            _parentNode?.List?.Remove(_parentNode);
            newParent._children ??= new LinkedList<Entity>();
            _parentNode         =   newParent._children.AddLast(this);
            this.App            =   newParent.App;
        }
    }

    public Entity()
    {
    }

    public Entity(App app)
    {
        this.App = app;
    }

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

    public sealed override void Dispose()
    {
        // 已经销毁
        if (Version == 0)
            return;
        Version = 0;
        OnDestroy();
        foreach (var comp in _components.Values)
        {
            if (comp is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}