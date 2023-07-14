using System.IO.Pipelines;
using System.Net.WebSockets;
using Chuan;
using CraftNet.Services;

namespace Demo.Network;

public class WebSocketClient : WebSocketConnection
{
    private static readonly Dictionary<ushort, object> Handlers = new();

    private ClientWebSocket _clientWebSocket;

    // rpc 回调
    private readonly Dictionary<uint, IResponseCompletionSource<IResponse>> _callbacks = new();
    private static   uint                                                   lastUsedRpcId;
    public static uint GetNextRpcId() => ++lastUsedRpcId;

    public static IMessageDescCollection MessageDescCollection;

    private XRandom _inRandom, _outRandom;

    public static async Task<WebSocketClient> ConnectAsync(string url)
    {
        ClientWebSocket clientWebSocket = new ClientWebSocket();
        await clientWebSocket.ConnectAsync(new Uri(url), CancellationToken.None);

        // 相对于socket的输入(应用层的写)
        // PipeOptions inputOptions = new PipeOptions(writerScheduler:);

        // 相对于socket的输出(应用层的读)
        // PipeOptions outputOptions = new PipeOptions(readerScheduler:);

        var client = new WebSocketClient(clientWebSocket);
        client._clientWebSocket = clientWebSocket;
        client.Start();
        return client;
    }

    private WebSocketClient(WebSocket webSocket) : base(webSocket)
    {
    }

    public override ValueTask OnReceive(PipeReader input)
    {
        if (!input.TryRead(out ReadResult result))
            return ValueTask.CompletedTask;

        var buffer = result.Buffer;

        if (!NetPacketHelper.TryParse(MessageDescCollection, ref buffer, ref _inRandom, out NetPacket packet))
            return ValueTask.CompletedTask;

        lock (_callbacks)
        {
            // 分发消息，推送到主线程或让管道的Output调度到主线程。不然_callbacks字典(普通字典)就会出现线程安全问题。
            if (packet.MsgType == MessageType.Response)
            {
                if (_callbacks.TryGetValue(packet.RpcId, out IResponseCompletionSource<IResponse> tcs))
                {
                    _callbacks.Remove(packet.RpcId);
                    tcs.Complete((IResponse)packet.Body);
                }
            }
            else
            {
                Handlers.TryGetValue(packet.Opcode, out object obj);
            }
        }

        return ValueTask.CompletedTask;
    }

    protected override void OnSend(PipeWriter output, in WaitSendPacket packet)
    {
        NetPacketHelper.Send(MessageDescCollection, output, in packet);
    }

    public override async ValueTask DisposeAsync()
    {
        try
        {
            _ = _clientWebSocket.CloseAsync(WebSocketCloseStatus.Empty, "", CancellationToken.None);
        }
        finally
        {
            await base.DisposeAsync();
        }
    }

    public void Send<T>(T msg) where T : IMessage, IMessageMeta
    {
        ushort opcode = T.Opcode;
        this.Send(opcode, null, msg, _outRandom.RandomInt());
    }

    public ValueTask<IResponse> Call<T>(T req) where T : IRequest, IMessageMeta
    {
        uint   rpcId  = GetNextRpcId();
        ushort opcode = T.Opcode;

        this.Send(opcode, rpcId, req, _outRandom.RandomInt());

        ResponseCompletionSource<IResponse> tcs = ResponseCompletionSourcePool.Get<IResponse>();

        lock (_callbacks) // 客户端直接lock
        {
            _callbacks.TryAdd(rpcId, tcs);
        }

        _ = Transport.Output.FlushAsync().Result;

        return tcs.AsValueTask();
    }
}