using System.Net.WebSockets;
using Demo.Network;
using CraftNet;
using CraftNet.Services;

namespace Demo;

public class WebSocketService : WebSocketListener<DemoWsServerConnection>
{
    private static int _index = 0;

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
        IReadOnlyList<IApp> gateApps    = AppList.Ins[AppType.Gate];
        int                 targetIndex = (Interlocked.Increment(ref _index) + 1) % gateApps.Count;
        IApp                app         = gateApps[targetIndex];
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