using System.Net;

namespace XGFramework.Services;

public interface ISenderManager
{
    /// <summary>
    /// 添加远程ip
    /// </summary>
    /// <param name="pid"></param>
    /// <param name="endpoint"></param>
    void AddRemote(ushort pid, IPEndPoint endpoint);

    void RemoveRemote(ushort pid);

    bool TryGet(ushort pid, out IMessageSender sender);

    IMessageSender GetOrCreate(ushort pid);
}