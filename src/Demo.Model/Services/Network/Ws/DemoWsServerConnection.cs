using System.IO.Pipelines;
using System.Net.WebSockets;
using Demo.Network;
using CraftNet;
using CraftNet.Services;

namespace Demo;

public class DemoWsServerConnection : WebSocketConnection
{
    public readonly  IApp                   App;
    private readonly IActorService          _actorService;
    private          ActorId                _agentActorId;
    private readonly IMessageDescCollection _messageDescCollection;

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
        if (!NetPacketParser.TryParse(_messageDescCollection, ref buffer, out NetPacket packet)) return;

        // 投递(线程安全)
        _actorService.Post(_agentActorId, packet.MsgType, packet.Opcode, packet.RpcId, packet.Body,
            extra: 1);
    }

    protected override void OnSend(PipeWriter output, in WaitSendPacket packet)
    {
        NetPacketParser.Send(_messageDescCollection, output, packet);
    }

    public async Task RunAsync()
    {
        _agentActorId =
            await EventSystemHelper.InvokeAsync<EventConnectedAgent, ActorId>(this.App,
                new EventConnectedAgent(null, SendToClient));
        // 开始接收数据
        await StartAsync();
        await EventSystemHelper.TriggerAsync(this.App, new EventDisconnectedAgent());
    }

    /// <summary>
    /// 来自分配的App相关线程
    /// </summary>
    /// <param name="agent"></param>
    /// <param name="opcode"></param>
    /// <param name="rpcId"></param>
    /// <param name="body"></param>
    private void SendToClient(Agent agent, ushort opcode, uint? rpcId, object body)
    {
        this.Send(opcode, rpcId, body);
    }
}