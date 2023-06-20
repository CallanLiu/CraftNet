using System.Collections.Concurrent;
using System.Net;
using Microsoft.AspNetCore.Connections;
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
    private readonly IConnectionFactory                        _connectionFactory;
    private readonly ILocalPId                                 _localPId;

    public SenderManager(IConnectionFactory connectionFactory, ILocalPId localPId)
    {
        _connectionFactory = connectionFactory;
        _localPId          = localPId;
    }

    public void AddServer(ushort pid, IPEndPoint endpoint)
    {
        // 运行时。一个pid始终对应一个固定的endpoint，不会动态转移pid对应的endpoint。
        if (!_senders.TryAdd(pid, new SenderEntry() { EndPoint = endpoint, Sender = null }))
        {
            throw new ArgumentException($"pid已存在: {pid}, ip={endpoint}");
        }
    }

    public void RemoveServer(ushort pid)
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
            entry.Sender = new MessageSender(_localPId.Value, pid, entry.EndPoint, _connectionFactory);
            return entry.Sender;
        }
    }
}