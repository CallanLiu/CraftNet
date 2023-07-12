using System.Diagnostics;
using System.Reflection;
using System.Runtime.Loader;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Primitives;
using Serilog;

namespace CraftNet;

/// <summary>
/// 插件程序集上下文
/// </summary>
public class PluginAssemblyContext : IPluginAssemblyContext
{
    public string Name { get; }

    public Assembly ModelAssembly { get; private set; }
    public Assembly HotfixAssembly { get; private set; }

    private Action<App> _onLoad;

    public event Action OnAssemblyReloadEvent;

    private AssemblyLoadContext _assemblyLoadContext;

    private readonly PhysicalFileProvider _physicalFileProvider;

    private uint                    lastChangeTime;
    private CancellationTokenSource _cts;

    private string HotfixDllName => $"{Name}.Hotfix.dll";

    public PluginAssemblyContext(string name, PhysicalFileProvider physicalFileProvider)
    {
        Name                  = name;
        _physicalFileProvider = physicalFileProvider;
    }

    public void Load()
    {
        // if (File.Exists($"{Name}.Model.dll"))
        // {
        //     ModelAssembly =
        //         AssemblyLoadContext.Default.LoadFromAssemblyPath(Path.Combine(Path.GetFullPath("."),
        //             $"{Name}.Model.dll"));
        // }

        if (LoadHotfix())
        {
            // 监听热更新程序集
            ChangeToken.OnChange(() => _physicalFileProvider.Watch(HotfixDllName), this.OnChanged);
        }
        else
        {
            Log.Warning("插件程序集不存在: {Name}", HotfixDllName);
        }
    }

    public void InvokeEntryPoint(object arg)
    {
        _onLoad?.Invoke(arg as App);
    }

    private async void OnChanged()
    {
        try
        {
            // 取消前面的
            // _cts?.Cancel();
            // _cts?.Dispose();
            // _cts = new CancellationTokenSource();

            uint nowSeconds = (uint)(Stopwatch.GetTimestamp() / TimeSpan.TicksPerSecond);
            Console.WriteLine($"热更新程序集变动: {this.Name} 线程Id={Thread.CurrentThread.ManagedThreadId} {nowSeconds}");
            if (lastChangeTime != nowSeconds)
            {
                lastChangeTime = (uint)(Stopwatch.GetTimestamp() / TimeSpan.TicksPerSecond);

                await Task.Delay(5000);
                LoadHotfix();
                OnAssemblyReloadEvent?.Invoke();
            }
        }
        catch (Exception e)
        {
            Console.WriteLine(e);
        }
    }

    private bool LoadHotfix()
    {
        if (!File.Exists(HotfixDllName)) return false;

        _onLoad = null;
        // OnLoadCompleted = null;
        HotfixAssembly = null;

        _assemblyLoadContext?.Unload();
        _assemblyLoadContext = new AssemblyLoadContext(Name, true);

        using var dll = File.OpenRead(HotfixDllName);
        using var pdb = File.OpenRead($"{Name}.Hotfix.pdb");
        HotfixAssembly = _assemblyLoadContext.LoadFromStream(dll, pdb);
        Type type = HotfixAssembly.GetType($"{Name}.Main");
        if (type != null)
        {
            MethodInfo loadMethodInfo = type.GetMethod("OnLoad",
                BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            _onLoad = loadMethodInfo?.CreateDelegate<Action<App>>();

            // MethodInfo loadCompletedMethodInfo = type.GetMethod("OnLoadCompleted",
            //     BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
            // OnLoadCompleted = loadCompletedMethodInfo?.CreateDelegate<Action<Setup>>();
        }

        Log.Information("载入热更程序集: {Name}", HotfixDllName);
        return true;
    }
}