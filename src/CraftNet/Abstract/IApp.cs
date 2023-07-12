namespace CraftNet;

/*
 * 框架名称: CraftNet
 * 框架分层:
 * 
 *  [Plugin] 插件层: 可热更，只有逻辑；由各个业务子系统组成。
 * 
 *  [App] 业务功能层: 由多个App实例组成，每个App内使用单线程模型；如：登录服、网关服、游戏服等等...
 *
 *  [Service] 服务层: 提供给上层的服务(需要考虑线程安全), 跨越多个App。
 */
public interface IApp
{
    public AppId Id { get; }

    public string Name { get; }

    /// <summary>
    /// 版本(每次Reload后加一)
    /// </summary>
    public uint Version { get; }

    /// <summary>
    /// 调度器
    /// </summary>
    public IAppScheduler Scheduler { get; }
}