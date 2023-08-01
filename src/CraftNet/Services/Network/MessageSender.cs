using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Runtime.CompilerServices;
using CraftNet.Services;
using MemoryPack;
using Microsoft.AspNetCore.Connections;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace CraftNet;

/// <summary>
/// 队列单线程
/// </summary>
public class MessageSender : IMessageSender
{
    const long TimeoutMinute = 1 * TimeSpan.TicksPerMinute; // 1分钟超时

    readonly record struct RpcCallbackItem(IResponseCompletionSource<IResponse> Tcs, long SendTime);

    private readonly VThread _vThread;

    private readonly ushort _localPId;

    public ushort PId { get; }

    public readonly  IPEndPoint         RemoteEntPoint;
    private readonly IConnectionFactory _connectionFactory;

    private ConnectionContext _connectionContext;
    private bool              _isConnecting = false;

    private readonly Action<object>                    _onTimerCallback;
    private readonly Action<object>                    _responseExecCallback;
    private readonly Action<object>                    _sendExecCallback;
    private readonly Queue<ActorMessage>               _queue;
    private readonly IMessageDescCollection            _messageDescCollection;
    private readonly Dictionary<uint, RpcCallbackItem> _callbacks;
    private readonly Queue<(uint, long)>               _timeoutQueue;

    public MessageSender(ushort localPId, ushort pid, IPEndPoint endpoint, IServiceProvider services)
    {
        _localPId              = localPId;
        this.PId               = pid;
        RemoteEntPoint         = endpoint;
        _vThread               = new VThread();
        _sendExecCallback      = SendExecCallback;
        _responseExecCallback  = ResponseExecCallback;
        _queue                 = new Queue<ActorMessage>();
        _callbacks             = new();
        _onTimerCallback       = OnTimeout;
        _connectionFactory     = services.GetService<IConnectionFactory>();
        _messageDescCollection = services.GetService<IMessageDescCollection>();

        _timeoutQueue = new Queue<(uint, long)>();
        ITimerService timerService = services.GetService<ITimerService>();
        timerService.AddInterval(0, 1000, OnTimerCallback, null);
    }

    private void OnTimerCallback(ITimer timer)
    {
        _vThread.Schedule(_onTimerCallback, timer);
    }

    public void Send(ActorMessage message) => _vThread.Schedule(_sendExecCallback, message);
    public void OnResponse(ActorMessage message) => _vThread.Schedule(_responseExecCallback, message);

    private void SendExecCallback(object obj)
    {
        ActorMessage message = (ActorMessage)obj;
        message.SenderPId = _localPId;
        if (message.Type == MessageType.Request)
        {
            var rpcCallbackItem = new RpcCallbackItem(message.Tcs, Stopwatch.GetTimestamp());
            if (_callbacks.TryAdd(message.RpcId, rpcCallbackItem))
            {
                _timeoutQueue.Enqueue((message.RpcId, rpcCallbackItem.SendTime));
                _queue.Enqueue(message);
            }
            else // rpcId溢出归零后，但字典中还有相同key就直接抛出异常。
            {
                message.Tcs.SetException(RpcIdOverflowException.Default);
                return;
            }
        }
        else
        {
            _queue.Enqueue(message);
        }

        if (_connectionContext != null)
            SendAll();
        else if (!_isConnecting)
            _ = ConnectAsync();
    }

    private void ResponseExecCallback(object obj)
    {
        ActorMessage message = (ActorMessage)obj;
        if (_callbacks.TryGetValue(message.RpcId, out var result))
        {
            var (tcs, _) = result;
            _callbacks.Remove(message.RpcId);

            if (message.Opcode is 0 && message.Body is ExceptionResponse exceptionResponse)
            {
                tcs.SetException(exceptionResponse);
            }
            else
            {
                tcs.Complete((IResponse)message.Body);
            }
        }
    }

    private void SendAll()
    {
        // 先发送队列缓存的
        var output = this._connectionContext.Transport.Output;
        while (_queue.TryDequeue(out ActorMessage message))
        {
            if (!_messageDescCollection.TryGet(message.Opcode, out MessageDesc messageDesc))
            {
                if (message.Type.HasRequest()) // rpc可以给它响应一个异常
                {
                    OnResponse(ActorMessage.CreateResponse(default, 0, message.RpcId,
                        new ExceptionResponse($"消息描述不存在,无法发送(不能序列化): {message.Body.GetType().Name}")));
                }
                else // msg就只能打印了
                {
                    Log.Error("消息描述不存在,无法发送(不能序列化): {Name}", message.Body.GetType().Name);
                }

                return;
            }

            // 反转为当前pid
            ActorId actorId = message.ActorId;

            // 预留长度字段
            Span<byte> headSpan = output.GetSpan(4);
            output.Advance(4);

            // 写入actorId
            WriteUnmanaged(output, actorId.PId);
            WriteUnmanaged(output, actorId.Type);
            WriteUnmanaged(output, actorId.Key);

            // 写入发送PID
            WriteUnmanaged(output, message.SenderPId);

            // 写消息信息
            WriteUnmanaged(output, message.Type);
            WriteUnmanaged(output, message.Opcode);
            if (message.Type.HasRpcField())
                WriteUnmanaged(output, message.RpcId);

            // 写入body
            messageDesc.Serializer.Serialize(message.Body, output);

            // 写入长度字段
            BinaryPrimitives.WriteUInt32LittleEndian(headSpan, (uint)(output.UnflushedBytes - 4));
            _ = output.FlushAsync();
        }

        static void WriteUnmanaged<T1>(IBufferWriter<byte> writer, scoped in T1 v1) where T1 : unmanaged
        {
            int        num  = Unsafe.SizeOf<T1>();
            Span<byte> span = writer.GetSpan(num);
            Unsafe.WriteUnaligned(ref span[0], v1);
            writer.Advance(num);
        }
    }

    private void OnConnected(object connectionContext)
    {
        _isConnecting      = false;
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

        _connectionContext = null;
        _isConnecting      = false;
    }

    private async ValueTask ConnectAsync()
    {
        try
        {
            _isConnecting = true;

            // 连接
            var conn = await _connectionFactory.ConnectAsync(this.RemoteEntPoint);
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
    }

    private void OnTimeout(object state)
    {
        long now = Stopwatch.GetTimestamp();

        while (_timeoutQueue.TryPeek(out var result))
        {
            var (key, sendTime) = result;

            // 当rpcId已经不存在时
            if (!_callbacks.TryGetValue(key, out var value))
            {
                _timeoutQueue.Dequeue();
                continue;
            }

            // 最先发送的都还没过期
            if (now - sendTime < TimeoutMinute)
                break;

            _timeoutQueue.Dequeue();
            _callbacks.Remove(key);
            value.Tcs.SetException(RpcTimeoutException.Default);
        }
    }
}