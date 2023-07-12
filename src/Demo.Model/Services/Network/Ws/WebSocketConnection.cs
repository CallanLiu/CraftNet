using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Threading.Channels;
using Serilog;
using CraftNet;

namespace Demo.Network;

public record struct WaitSendPacket(ushort Opcode, uint? RpcId, object Body);

/// <summary>
/// 封装对WebSocket基础读写
/// </summary>
public abstract class WebSocketConnection : IAsyncDisposable
{
    public uint Id { get; protected set; }

    public WebSocket WebSocket { get; }

    // 读取/写入传输层
    public IDuplexPipe Transport { get; }

    // 读取/写入应用层
    private IDuplexPipe Application { get; }

    public virtual CancellationToken ConnectionClosed { get; }

    private          CancellationTokenSource _cts;
    private          Task                    _receiveTask, _sendTask;
    private readonly Channel<WaitSendPacket> _channel; // 因为ws自动处理分包，需要保证每个消息依次发送。就不能直接使用Pipe往里写。

    public WebSocketConnection(WebSocket webSocket, PipeOptions inputOptions = null, PipeOptions outputOptions = null,
        UnboundedChannelOptions channelOptions = null)
    {
        WebSocket = webSocket;

        // 默认使用线程池调度
        var pair = DuplexPipe.CreateConnectionPair(inputOptions ?? PipeOptions.Default,
            outputOptions ?? PipeOptions.Default);

        this.Transport   = pair.Transport;
        this.Application = pair.Application;

        _cts             = new CancellationTokenSource();
        ConnectionClosed = _cts.Token;

        _channel = channelOptions != null
            ? Channel.CreateUnbounded<WaitSendPacket>(channelOptions)
            : Channel.CreateUnbounded<WaitSendPacket>();
    }

    public void Start()
    {
        _receiveTask = DoReceive();
        _sendTask    = DoSend();
    }

    public Task StartAsync()
    {
        Start();
        return Task.WhenAll(_receiveTask, _sendTask);
    }

    private async Task DoReceive()
    {
        await Task.Yield();

        try
        {
            PipeWriter output = this.Application.Output; // 写给应用层的数据
            WebSocket  ws     = WebSocket;
            PipeReader input  = this.Transport.Input;

            while (!ws.CloseStatus.HasValue)
            {
                Memory<byte> memory = output.GetMemory(2048);
                ValueWebSocketReceiveResult result =
                    await ws.ReceiveAsync(memory, ConnectionClosed);
                output.Advance(result.Count);

                // 消息没有结束继续接受
                if (!result.EndOfMessage) continue;

                // 管道的异步延续默认情况是: 使用当前的 SynchronizationContext，如果没有 SynchronizationContext，它将使用线程池运行回调。(PipeScheduler.ThreadPool)
                // 所以此await后的代码将会在线程池中调用。
                // 但是把此管道的writerScheduler配置为PipeSchedulerWithIScheduler让其与actor公用一个调度器，就会又回到actor调度上线文。
                FlushResult flushResult = await output.FlushAsync(ConnectionClosed);
                if (flushResult.IsCanceled || flushResult.IsCompleted)
                    break;


                // 接着解析, 绝对有数据
                if (!input.TryRead(out ReadResult readResult)) continue;
                try
                {
                    OnReceive(in readResult);
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                }
                finally // 消费完
                {
                    input.AdvanceTo(readResult.Buffer.End);
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "接受数据异常:");
        }
        finally
        {
            this.Application.Output.Complete();
        }
    }

    public abstract void OnReceive(in ReadResult readResult);

    private async Task DoSend()
    {
        await Task.Yield();

        try
        {
            var output        = this.Transport.Output;
            var input         = this.Application.Input; // 来自应用层的输入
            var ws            = this.WebSocket;
            var channelReader = this._channel.Reader;
            while (true)
            {
                var more = await channelReader.WaitToReadAsync();
                if (!more)
                    break;

                while (channelReader.TryRead(out var item))
                {
                    OnSend(output, in item);

                    FlushResult flushResult = await output.FlushAsync();
                    if (flushResult.IsCompleted || flushResult.IsCanceled)
                        return;

                    // 绝对又数据
                    input.TryRead(out ReadResult result);
                    if (result.IsCanceled || result.IsCompleted) // 无所谓，不用发了。因为读写在断线时，会设置为已完成。
                        return;

                    var sendBuffer = result.Buffer;
                    if (sendBuffer.IsEmpty)
                        return;
                    if (sendBuffer.IsSingleSegment)
                    {
                        await ws.SendAsync(sendBuffer.First, WebSocketMessageType.Binary, true, ConnectionClosed);
                    }
                    else
                    {
                        // 多个
                        var  enumerator = sendBuffer.GetEnumerator();
                        bool end        = enumerator.MoveNext();
                        while (!end)
                        {
                            var currentBuffer = enumerator.Current;
                            end = enumerator.MoveNext(); // 没有下一个，当前就是结束消息。
                            await ws.SendAsync(currentBuffer, WebSocketMessageType.Binary, !end, ConnectionClosed);
                        }
                    }

                    input.AdvanceTo(sendBuffer.End);
                }
            }
        }
        catch (Exception e)
        {
            Log.Error(e, "WebSocket发送数据异常");
        }
        finally
        {
            Transport.Output.Complete();
        }
    }

    protected abstract void OnSend(PipeWriter output, in WaitSendPacket packet);

    public void Send(ushort opcode, uint? rpcId, object body)
    {
        _channel.Writer.TryWrite(new WaitSendPacket(opcode, rpcId, body));
    }

    public virtual async ValueTask DisposeAsync()
    {
        Transport.Input.Complete();
        Transport.Output.Complete();

        try
        {
            _cts.Cancel();

            if (_receiveTask != null)
            {
                await _receiveTask;
            }

            if (_sendTask != null)
            {
                await _sendTask;
            }

            // await WebSocket.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
        }
        catch (Exception e)
        {
            Log.Error(e, "");
        }
        finally
        {
            WebSocket?.Dispose();
        }

        _cts.Dispose();
    }
}