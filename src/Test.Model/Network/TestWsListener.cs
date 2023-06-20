using System.Buffers.Binary;
using System.IO.Pipelines;
using System.Net.WebSockets;
using System.Text.Json;
using Test;
using XGFramework;
using XGFramework.Services;

namespace Test;

public class TestWsServerConnection : WebSocketConnection
{
    public readonly  IApp            App;
    private readonly IActorService _actorService;
    private          Agent           _agent;

    public TestWsServerConnection(WebSocket webSocket, IApp app, IActorService actorService) : base(webSocket)
    {
        App             = app;
        _actorService = actorService;
    }

    public override void OnReceiveMessage(in ReadResult result)
    {
        var buffer = result.Buffer;
        if (!WsPacketParser.TryParse(ref buffer, out WsPacket packet)) return;

        // 投递(线程安全)
        _actorService.Post(_agent.ActorId, packet.MsgType, packet.Opcode, packet.RpcId, (IMessageBase)packet.Body,
            extra: 1);
    }

    protected override void OnSend(PipeWriter output, in WaitSendPacket packet)
    {
        // 发给客户端只有两种类型(IMessage与IResponse)
        if (packet.RpcId is null)
        {
            // 写入头: type[1]+opcode[2]
            Memory<byte> buffer = Transport.Output.GetMemory(3);
            buffer.Span[0] = MessageType.Message;
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Span[1..], packet.Opcode);
            Transport.Output.Advance(3);

            // 写入body
            Utf8JsonWriter jsonWriter = new Utf8JsonWriter(Transport.Output);
            JsonSerializer.Serialize(jsonWriter, packet.Body);
        }
        else
        {
            // 写入头: type[1]+opcode[2]+rpcId[4]
            Memory<byte> buffer = Transport.Output.GetMemory(7);
            buffer.Span[0] = MessageType.Response;
            BinaryPrimitives.WriteUInt16BigEndian(buffer.Span.Slice(1, 2), packet.Opcode);
            BinaryPrimitives.WriteUInt32BigEndian(buffer.Span.Slice(3, 4), packet.RpcId.Value);
            Transport.Output.Advance(7);

            // 写入body
            Utf8JsonWriter jsonWriter = new Utf8JsonWriter(Transport.Output);
            JsonSerializer.Serialize(jsonWriter, packet.Body);
        }
    }

    public async Task RunAsync()
    {
        var app = (App)App;
        await App.Scheduler.Yield();
        _agent = new Agent(this, this.Send);
        IEventSystem eventSystem = app.GetSystem<IEventSystem>();
        eventSystem.Trigger(new EventWebSocketConnected(_agent));

        // 等待连接断开
        await StartAsync();

        await app.Scheduler.Yield();
        eventSystem.Trigger(new EventWebSocketDisconnected(_agent));
    }
}

public class TestWsListener : WebSocketListener<TestWsServerConnection>
{
    // 提前放入
    public static  IApp[] Apps  = new IApp[1];
    private static int    index = 0;

    private readonly IActorService _actorService;

    public TestWsListener(IActorService actorService)
    {
        _actorService = actorService;
    }

    protected override TestWsServerConnection Create(WebSocket webSocket)
    {
        App app = (App)GetNextApp();
        return new TestWsServerConnection(webSocket, app, _actorService);
    }

    private static IApp GetNextApp()
    {
        int  targetIndex = (Interlocked.Increment(ref index) + 1) % Apps.Length;
        IApp app         = Apps[targetIndex];
        return app;
    }

    protected override async ValueTask OnConnectedAsync(TestWsServerConnection conn)
    {
        await conn.RunAsync();
    }

    public override async ValueTask DisposeAsync()
    {
    }
}