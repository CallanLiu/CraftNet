using System.Net.WebSockets;
using Demo.Network;
using CraftNet;
using CraftNet.Services;

namespace Demo;

public class WebSocketService : WebSocketListener<DemoWsServerConnection>
{
    // 提前放入
    public static readonly List<IApp> Apps   = new();
    private static         int        _index = 0;

    private readonly IMessageDescCollection _messageDescCollection;

    public WebSocketService(IMessageDescCollection messageDescCollection)
    {
        _messageDescCollection = messageDescCollection;
    }

    protected override DemoWsServerConnection Create(WebSocket webSocket)
    {
        App app = (App)GetNextApp();
        return new DemoWsServerConnection(webSocket, app, _messageDescCollection);
    }

    private static IApp GetNextApp()
    {
        int  targetIndex = (Interlocked.Increment(ref _index) + 1) % Apps.Count;
        IApp app         = Apps[targetIndex];
        return app;
    }

    protected override async ValueTask OnConnectedAsync(DemoWsServerConnection conn)
    {
        await conn.RunAsync();
    }

    public override async ValueTask DisposeAsync()
    {
    }
}