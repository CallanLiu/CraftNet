using CraftNet;

namespace Demo;

/// <summary>
/// 本地创建的所有app
/// </summary>
public class AppList : Singleton<AppList>
{
    private readonly Dictionary<int, List<IApp>> _apps   = new Dictionary<int, List<IApp>>();
    private readonly ReaderWriterLockSlim        _rwLock = new();

    public IReadOnlyList<IApp> this[AppType type]
    {
        get
        {
            try
            {
                _rwLock.EnterReadLock();
                _apps.TryGetValue((int)type, out var list);
                return list;
            }
            finally
            {
                _rwLock.ExitReadLock();
            }
        }
    }

    public void Add(AppType type, IApp app)
    {
        try
        {
            _rwLock.EnterWriteLock();

            if (!_apps.TryGetValue((int)type, out List<IApp> apps))
            {
                apps = new List<IApp>();
                _apps.Add((int)type, apps);
            }

            apps.Add(app);
        }
        finally
        {
            _rwLock.ExitWriteLock();
        }
    }
}