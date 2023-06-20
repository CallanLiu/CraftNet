using System.Buffers.Binary;
using System.Net;
using System.Text.Json;
using Microsoft.AspNetCore.Connections;
using Serilog;
using XGFramework.Services;

namespace XGFramework;

/// <summary>
/// 队列单线程
/// </summary>
public class MessageSender : IMessageSender
{
    private VThread _vThread;

    private readonly ushort _localPId;

    public ushort PId { get; }

    public readonly  IPEndPoint         RemoteEntPoint;
    private readonly IConnectionFactory _connectionFactory;

    private ConnectionContext _connectionContext;

    private readonly Action<object>      _responseExecCallback;
    private readonly Action<object>      _sendExecCallback;
    private readonly Queue<ActorMessage> _queue;

    private readonly Dictionary<uint, IResponseCompletionSource<IResponse>> _callbacks;

    public MessageSender(ushort localPId, ushort pid, IPEndPoint endpoint, IConnectionFactory connectionFactory)
    {
        _localPId             = localPId;
        this.PId              = pid;
        RemoteEntPoint        = endpoint;
        _connectionFactory    = connectionFactory;
        _vThread              = new VThread();
        _sendExecCallback     = SendExecCallback;
        _responseExecCallback = ResponseExecCallback;
        _queue                = new Queue<ActorMessage>();
        _callbacks            = new Dictionary<uint, IResponseCompletionSource<IResponse>>();

        _ = ConnectAsync(endpoint);
    }

    public void Send(ActorMessage message) => _vThread.Schedule(_sendExecCallback, message);
    public void OnResponse(ActorMessage message) => _vThread.Schedule(_responseExecCallback, message);

    private void SendExecCallback(object obj)
    {
        ActorMessage message = (ActorMessage)obj;
        _queue.Enqueue(message);
        if (message.Type == MessageType.Request)
            _callbacks.Add(message.RpcId, message.Tcs);
        if (_connectionContext != null)
            SendAll();
    }

    private void ResponseExecCallback(object obj)
    {
        ActorMessage message = (ActorMessage)obj;
        if (_callbacks.TryGetValue(message.RpcId, out var tcs))
        {
            _callbacks.Remove(message.RpcId);
            tcs.Complete((IResponse)message.Body);
        }
    }

    private void SendAll()
    {
        // 先发送队列缓存的
        var output = this._connectionContext.Transport.Output;
        while (_queue.TryDequeue(out ActorMessage message))
        {
            // 反转为当前pid
            ActorId    inverseActorId = new ActorId(_localPId, message.ActorId.Index);
            Span<byte> headSpan       = MessageHeaderHelper.WriteHead(message, output, inverseActorId);

            // 发送body
            Utf8JsonWriter jsonWriter = new Utf8JsonWriter(output);
            JsonSerializer.Serialize(jsonWriter, message.Body);

            // 写入长度字段
            BinaryPrimitives.WriteInt32BigEndian(headSpan, (int)(output.UnflushedBytes - 4));
            _ = output.FlushAsync();
        }
    }

    private void OnConnected(object connectionContext)
    {
        _connectionContext = (ConnectionContext)connectionContext;
        Log.Debug("连接服务端成功: pid={PId} ip={IP}", PId, this.RemoteEntPoint);
        SendAll();
    }

    private void OnDisconnected(object connectionContext)
    {
        this._connectionContext = null;
        Log.Debug("与服务端断开连接: pid={PId} ip={IP}", PId, this.RemoteEntPoint);
    }

    private void OnConnectionFail(object e)
    {
        while (_queue.TryDequeue(out ActorMessage message))
        {
            if (message.Type is MessageType.Request && message.Tcs is not null)
            {
                message.Tcs.SetException((Exception)e);
            }
        }
    }

    private async ValueTask ConnectAsync(IPEndPoint endpoint)
    {
        while (true)
        {
            try
            {
                // 连接
                var conn = await _connectionFactory.ConnectAsync(endpoint);
                _vThread.Schedule(OnConnected, conn);
                TaskCompletionSource tcs = new TaskCompletionSource();
                conn.ConnectionClosed.Register(() => tcs.SetResult());
                await tcs.Task;
                _vThread.Schedule(OnDisconnected, conn);
            }
            catch (Exception e)
            {
                Log.Error(e, "内部Socket:");
                _vThread.Schedule(OnConnectionFail, e);
            }
            finally
            {
                Log.Debug($"连接已断开: 2s后将重连...");
            }

            await Task.Delay(2000);
        }
    }
}