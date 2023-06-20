using System.Net;

namespace XGFramework.Services;

public interface ISenderManager
{
    /// <summary>
    /// 添加一台服务器
    /// </summary>
    /// <param name="pid"></param>
    /// <param name="endpoint"></param>
    void AddServer(ushort pid, IPEndPoint endpoint);

    void RemoveServer(ushort pid);

    bool TryGet(ushort pid, out IMessageSender sender);

    IMessageSender GetOrCreate(ushort pid);
}