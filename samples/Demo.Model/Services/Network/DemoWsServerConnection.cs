using System.IO.Pipelines;
using System.Net.WebSockets;
using Demo.Network;
using CraftNet;
using CraftNet.Services;
using Demo.Services.Network;

namespace Demo;

public class DemoWsServerConnection : WebSocketConnection
{
    public readonly  IApp                   App;
    private          Session                _session;
    private readonly IMessageDescCollection _messageDescCollection;

    public DemoWsServerConnection(WebSocket webSocket, IApp app, IMessageDescCollection messageDescCollection) :
        base(webSocket)
    {
        App                    = app;
        _messageDescCollection = messageDescCollection;
    }

    #region 网络线程

    public async Task RunAsync()
    {
        // 创建Session
        _session =
            await EventHelper.InvokeAsync<NetworkConnectedFunc, Session>(this.App,
                new NetworkConnectedFunc(null, this.SendToClient));

        // 开始接收数据
        await StartAsync();

        // 切换到app上下文，然后在操作session。
        await App.Scheduler.Yield();
        _session.Dispose();
    }

    /// <summary>
    /// 来自网络线程
    /// </summary>
    /// <param name="input"></param>
    public override async ValueTask OnReceive(PipeReader input)
    {
        if (!input.TryRead(out ReadResult result))
            return;

        var buffer = result.Buffer;

        // 切到App的调度上下文进行Session操作
        await App.Scheduler.Yield();

        // 解析消息
        if (!NetPacketHelper.TryParse(_messageDescCollection, ref buffer, ref _session.InRandom, out NetPacket packet))
        {
            _session.Dispose();
            return;
        }

        // 已经处于线程安全环境，直接给入队消息即可。
        _session.Actor.UnsafePost(new ActorMessage(packet.MsgType, _session.Actor.ActorId, packet.Opcode, packet.Body,
            packet.RpcId, direction: 1));
    }

    protected override void OnSend(PipeWriter output, in WaitSendPacket packet)
    {
        NetPacketHelper.Send(_messageDescCollection, output, packet);
    }

    #endregion

    /// <summary>
    /// 来自Session所在的App线程(安全可直接访问session)
    /// </summary>
    /// <param name="opcode"></param>
    /// <param name="rpcId"></param>
    /// <param name="body"></param>
    private void SendToClient(ushort opcode, uint? rpcId, object body)
    {
        uint key = _session.OutRandom.RandomInt();
        this.Send(opcode, rpcId, body, key);
    }
}