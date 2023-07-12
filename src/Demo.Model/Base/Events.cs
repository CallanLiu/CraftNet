using System.Net;
using CraftNet;

namespace Demo;

public record struct EventSessionConnected(IPEndPoint RemoteEndPoint, SendToClientAction SendToClientAction) : IEvent;

public record struct EventSessionDisconnected() : IEvent;