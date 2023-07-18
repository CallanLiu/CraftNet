using Microsoft.Extensions.FileProviders;

namespace CraftNet;

public class PluginAssemblyService : IPluginAssemblyService
{
    private readonly Dictionary<string, PluginAssemblyContext> _pluginAssemblyContexts =
        new Dictionary<string, PluginAssemblyContext>();

    private readonly PhysicalFileProvider _fileProvider;

    public PluginAssemblyService(List<string> list)
    {
        _fileProvider = new PhysicalFileProvider(Path.GetFullPath("."));
        foreach (var str in list)
        {
            var ctx = new PluginAssemblyContext(str, _fileProvider);
            _pluginAssemblyContexts.Add(str, ctx);
            ctx.Load();
        }

        // 监听程序集变动
        // FileSystemWatcher fileSystemWatcher = new FileSystemWatcher(Path.GetFullPath("."), "*.dll");
        // fileSystemWatcher.NotifyFilter        =  NotifyFilters.LastWrite;
        // fileSystemWatcher.EnableRaisingEvents =  true;
        // fileSystemWatcher.Changed             += FileSystemWatcherOnChanged;
    }

    private void FileSystemWatcherOnChanged(object sender, FileSystemEventArgs e)
    {
        Console.WriteLine($"文件变动: {e.FullPath}, {e.ChangeType}");
    }

    public IPluginAssemblyContext Get(string name)
    {
        _pluginAssemblyContexts.TryGetValue(name, out PluginAssemblyContext ctx);
        return ctx;
    }
}