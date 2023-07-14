using Microsoft.Extensions.DependencyInjection;

namespace CraftNet;

public partial class App : ISystemCollection
{
    // 分组的系统
    private readonly Dictionary<uint, List<ISystemBase>> _groups = new();

    // 无序
    private ISystemBase[] _systems;

    // 有序的
    private readonly List<ISystemBase> _sortedSystems = new();

    public IReadOnlyList<ISystemBase> AllSystems => _sortedSystems;

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

        T t = ActivatorUtilities.CreateInstance<TImpl>(this.Services, this);
        _systems[index] = t;
        if (!_groups.TryGetValue(group, out var list))
        {
            list = new List<ISystemBase>();
            _groups.Add(group, list);
        }

        list.Add(t);
        _sortedSystems.Add(t);
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

    public IReadOnlyList<ISystemBase> GetSystems(uint group = 0)
    {
        _groups.TryGetValue(group, out var list);
        return list;
    }
}