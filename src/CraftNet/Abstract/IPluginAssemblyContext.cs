using System.Reflection;

namespace CraftNet;

public interface IPluginAssemblyContext
{
    string Name { get; }
    Assembly ModelAssembly { get; }
    Assembly HotfixAssembly { get; }

    event Action OnAssemblyReloadEvent;

    void Load();

    void InvokeEntryPoint(object arg);
}