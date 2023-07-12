using CraftNet;

namespace Demo.Entities;

public class Entity : GObject, IComponentCollection
{
    private readonly ComponentCollection _components = new ComponentCollection();

    public T GetComponent<T>() => _components.GetComponent<T>();
    public T AddComponent<T>() where T : new() => _components.AddComponent<T>();
    public T AddComponent<T>(T ins) => _components.AddComponent(ins);
    public bool RemoveComponent<T>() => _components.RemoveComponent<T>();

    public sealed override void Dispose()
    {
        // 已经销毁
        if (Version == 0)
            return;
        Version = 0;
        OnDestroy();
        foreach (var comp in _components.GetAllComponent())
        {
            if (comp is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }
    }
}