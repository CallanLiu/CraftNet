using System.Net.WebSockets;
using Microsoft.AspNetCore.Http;
using Serilog;

namespace Demo.Network;

public interface IWebSocketListener
{
    Task Accept(HttpContext context);
}

public abstract class WebSocketListener<TConn> : IWebSocketListener, IAsyncDisposable where TConn : WebSocketConnection
{
    public async Task Accept(HttpContext context)
    {
        if (!context.WebSockets.IsWebSocketRequest)
        {
            context.Response.StatusCode = StatusCodes.Status400BadRequest;
            return;
        }

        TConn connection = null;
        try
        {
            using var ws = await context.WebSockets.AcceptWebSocketAsync();
            connection = Create(ws);
            await OnConnectedAsync(connection);
        }
        catch (Exception e)
        {
            Log.Error(e, "WebSocket异常:");
        }
        finally
        {
            if (connection != null)
            {
                await connection.DisposeAsync();
            }
        }
    }

    protected abstract TConn Create(WebSocket webSocket);
    protected abstract ValueTask OnConnectedAsync(TConn connection);

    public abstract ValueTask DisposeAsync();
}