using System.Net;
using Demo.Entities;
using CraftNet.Services;

namespace Demo;

public delegate void SendToClientAction(ushort opcode, uint? rpcId, object body);

public class Session : Entity
{
    private readonly SendToClientAction _sendToClient;

    /// <summary>
    /// 客户端的IP地址
    /// </summary>
    public IPEndPoint ClientAddress { get; private set; }

    /// <summary>
    /// 与Session相关的ActorId
    /// </summary>
    public ActorId ActorId { get; set; }

    public Session(IPEndPoint clientAddress, SendToClientAction sendToClient)
    {
        this._sendToClient = sendToClient;
        this.ClientAddress = clientAddress;
    }

    /// <summary>
    /// 给客户端发送消息
    /// </summary>
    /// <param name="msg"></param>
    /// <typeparam name="T"></typeparam>
    public void Send<T>(T msg) where T : IMessage, IMessageMeta => Send(T.Opcode, msg);

    /// <summary>
    /// 手动指定opcode给客户端发送消息
    /// </summary>
    /// <param name="opcode"></param>
    /// <param name="message"></param>
    public void Send(ushort opcode, IMessage message) => _sendToClient?.Invoke(opcode, null, message);

    /// <summary>
    /// 给客户端回应一个消息(回应Request)
    /// </summary>
    /// <param name="opcode"></param>
    /// <param name="rpcId"></param>
    /// <param name="response"></param>
    public void Reply(ushort opcode, uint rpcId, IResponse response) =>
        _sendToClient?.Invoke(opcode, rpcId, response);
}