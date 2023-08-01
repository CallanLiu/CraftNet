using CraftNet;
using Demo.Services.Network;
using Serilog;

namespace Demo.Events;

public class NetworkConnectedFuncImpl : FuncImpl<NetworkConnectedFunc, Session>
{
    protected override Session On(in NetworkConnectedFunc e)
    {
        IGateSrv gateSrv = this.App.GetSystem<IGateSrv>();
        Session  session = gateSrv.CreateSession(e.RemoteEndPoint, e.SendToClientAction);
        if (session is null)
            return null;
        Log.Debug("连接成功: {Ws}", e.RemoteEndPoint);
        return session;
    }
}