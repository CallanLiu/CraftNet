using System.IO.Pipelines;
using System.Net.WebSockets;
using Chuan;
using Demo.Network;
using CraftNet;
using CraftNet.Services;

namespace Demo;

public class DemoWsServerConnection : WebSocketConnection
{
    public readonly  IApp                   App;
    private readonly IActorService          _actorService;
    private          ActorId                _sessionActorId;
    private readonly IMessageDescCollection _messageDescCollection;

    // 加密opcode
    private XRandom _outRandom;
    private XRandom _inRandom;

    public DemoWsServerConnection(WebSocket webSocket, IApp app, IActorService actorService,
        IMessageDescCollection messageDescCollection) : base(webSocket)
    {
        App                    = app;
        _actorService          = actorService;
        _messageDescCollection = messageDescCollection;
    }

    /// <summary>
    /// 来自网络线程
    /// </summary>
    /// <param name="result"></param>
    public override void OnReceive(in ReadResult result)
    {
        var buffer = result.Buffer;
        if (!NetPacketHelper.TryParse(_messageDescCollection, ref buffer, ref _inRandom, out NetPacket packet))
        {
            // 对于ws而言只有opcode不对，直接断掉。
            _ = this.DisposeAsync();
            return;
        }

        // 投递(线程安全)
        _actorService.Post(_sessionActorId, packet.MsgType, packet.Opcode, packet.RpcId, packet.Body,
            extra: 1);
    }

    protected override void OnSend(PipeWriter output, in WaitSendPacket packet)
    {
        NetPacketHelper.Send(_messageDescCollection, output, packet);
    }

    public async Task RunAsync()
    {
        _sessionActorId =
            await EventSystemHelper.InvokeAsync<EventSessionConnected, ActorId>(this.App,
                new EventSessionConnected(null, this.SendToClient));

        // 给客户端回应一个密钥，用于加密。
        uint defaultValue = _outRandom.RandomInt();
        int  seed         = Random.Shared.Next();
        _inRandom  = new XRandom((ulong)seed);
        _outRandom = new XRandom((ulong)seed);

        // 给客户端发送第一个数据
        this.Send(G2C_EncryptKeyMsg.Opcode, null, new G2C_EncryptKeyMsg { Key = seed }, defaultValue);

        // 开始接收数据
        await StartAsync();
        await EventSystemHelper.TriggerAsync(this.App, new EventSessionDisconnected());
    }

    private void SendToClient(ushort opcode, uint? rpcId, object body)
    {
        this.Send(opcode, rpcId, body, _outRandom.RandomInt());
    }
}