using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text.Json;
using Demo.Network;
using XGFramework;
using XGFramework.Services;

namespace Demo;

public class DemoWsServerConnection : WebSocketConnection
{
    public readonly  IApp          App;
    private readonly IActorService _actorService;
    private          Agent         _agent;

    public DemoWsServerConnection(WebSocket webSocket, IApp app, IActorService actorService) : base(webSocket)
    {
        App           = app;
        _actorService = actorService;
    }

    public override void OnReceiveMessage(in ReadResult result)
    {
        var buffer = result.Buffer;
        if (!WsPacket.TryParse(ref buffer, out WsPacket packet)) return;

        // 投递(线程安全)
        _actorService.Post(_agent.ActorId, packet.MsgType, packet.Opcode, packet.RpcId, (IMessageBase)packet.Body,
            extra: 1);
    }

    protected override void OnSend(PipeWriter output, in WaitSendPacket packet)
    {
        WsPacket.Send(output, packet);
    }

    public async Task RunAsync()
    {
        var app = (App)App;

        // 切换到app的调度上下文中
        await App.Scheduler.Yield();

        // 创建Agent
        _agent = new Agent(this, this.Send);
        IEventSystem eventSystem = app.GetSystem<IEventSystem>();
        eventSystem.Trigger(new EventWebSocketConnected(_agent));

        // 开始接收数据
        await StartAsync();

        // 连接断开，切换到app的调度上下文中
        await app.Scheduler.Yield();
        eventSystem.Trigger(new EventWebSocketDisconnected(_agent));
    }
}