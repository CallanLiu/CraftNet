using XGFramework;

namespace Demo;

public class AppList : Singleton<AppList>
{
    private Dictionary<string, List<IApp>> _apps = new Dictionary<string, List<IApp>>();

    public void Add(string type, IApp app)
    {
        if (!_apps.TryGetValue(type, out List<IApp> apps))
        {
            apps = new List<IApp>();
            _apps.Add(type, apps);
        }

        apps.Add(app);
    }
}