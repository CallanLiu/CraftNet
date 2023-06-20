namespace XGFramework;

/// <summary>
/// 程序集服务
/// </summary>
public interface IPluginAssemblyService
{
    IPluginAssemblyContext Get(string name);
}