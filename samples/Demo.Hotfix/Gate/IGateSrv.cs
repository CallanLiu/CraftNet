using System.Net;
using CraftNet;

namespace Demo;

/// <summary>
/// 网关服
/// </summary>
public interface IGateSrv : ISystemBase<IIDGateSvr>
{
    Session CreateSession(IPEndPoint remoteEndPoint, SendToClientAction sendToClientAction);
}