using System.Net;
using CraftNet;

namespace Demo;

public record struct EventConnectedAgent(IPEndPoint RemoteEndPoint, SendToClientAction SendToClientAction) : IEvent;

public record struct EventDisconnectedAgent() : IEvent;