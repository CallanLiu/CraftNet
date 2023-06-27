using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Connections;
using Serilog;
using XGFramework.Services;

namespace XGFramework;

public class SenderEntry
{
    public IPEndPoint    EndPoint;
    public MessageSender Sender;
}

public class SenderManager : ISenderManager
{
    private readonly object                                    _lockObj = new();
    private readonly ConcurrentDictionary<ushort, SenderEntry> _senders = new();
    private readonly ILocalPId                                 _localPId;
    private readonly IServiceProvider                          _services;

    public SenderManager(ILocalPId localPId, IServiceProvider services)
    {
        _localPId = localPId;
        _services = services;
    }

    public void AddRemote(ushort pid, IPEndPoint endpoint)
    {
        // 运行时。一个pid始终对应一个固定的endpoint，不会动态转移pid对应的endpoint。
        if (!_senders.TryAdd(pid, new SenderEntry() { EndPoint = endpoint, Sender = null }))
        {
            throw new ArgumentException($"pid已存在: {pid}, ip={endpoint}");
        }

        Log.Information("添加Sender成员: Pid={Pid}, IP={IP}", pid, endpoint);
    }

    public void RemoveRemote(ushort pid)
    {
        if (_senders.TryRemove(pid, out SenderEntry entry))
        {
            // entry.Sender?.Dispose();
        }
    }

    public bool TryGet(ushort pid, out IMessageSender sender)
    {
        if (_senders.TryGetValue(pid, out var result) && result is { Sender: not null })
        {
            sender = result.Sender;
            return true;
        }

        sender = null;
        return false;
    }

    public IMessageSender GetOrCreate(ushort pid)
    {
        lock (_lockObj)
        {
            // 有可用sender
            if (TryGet(pid, out IMessageSender sender))
                return sender;

            // 没有此PID的信息
            if (!_senders.TryGetValue(pid, out var entry))
                return null;

            // 创建
            entry.Sender = new MessageSender(_localPId.Value, pid, entry.EndPoint, _services);
            return entry.Sender;
        }
    }
}