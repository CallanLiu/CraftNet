using System.Collections;

namespace XGFramework;

public partial class App : ISystemCollection
{
    // 只提供快速获取(运行后无法再次改变顺序)
    private readonly Dictionary<uint, List<ISystemBase>> _groups  = new();
    private          ISystemBase[]                       _systems = null;

    public T AddSystem<T, TImpl>(uint group = 0) where T : ISystemBase
        where TImpl : T
    {
        int index = T.GetId();
        if (_systems is null || _systems.Length <= index)
        {
            var arr = new ISystemBase[index + 1];
            if (_systems is { Length: > 0 })
            {
                for (int i = 0; i < _systems.Length; i++)
                    arr[i] = _systems[i];
            }

            _systems = arr;
        }

        T t = (T)Activator.CreateInstance(typeof(TImpl), this);
        _systems[index] = t;
        if (!_groups.TryGetValue(group, out var list))
        {
            list = new List<ISystemBase>();
            _groups.Add(group, list);
        }

        list.Add(t);

        return t;
    }

    public T GetSystem<T>() where T : ISystemBase
    {
        int index = T.GetId();
        if (_systems is null)
            return default;
        if (_systems.Length <= index)
            return default;
        return (T)_systems[index];
    }

    public IReadOnlyList<ISystemBase> GetAllSystem(uint group = 0)
    {
        _groups.TryGetValue(group, out var list);
        return list;
    }

    public IEnumerator<ISystemBase> GetEnumerator()
    {
        if (_systems is null or { Length: 0 })
            yield break;

        // 使用组，里面的List将保证Add的顺序。
        foreach (var list in _groups.Values)
        {
            foreach (var item in list)
            {
                yield return item;
            }
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}