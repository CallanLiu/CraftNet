using System.Net;
using CraftNet;
using CraftNet.Services;

namespace Demo.Services.Network;

public record NetworkConnectedFunc
    (IPEndPoint RemoteEndPoint, SendToClientAction SendToClientAction) : ICallback<Session>;

public record struct NetworkDisconnectedAction(ushort Opcode, IMessageBase Msg, uint? RpcId) : ICallback;