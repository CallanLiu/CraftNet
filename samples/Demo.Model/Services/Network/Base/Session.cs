using System.Net;
using CraftNet;
using Demo.Entities;
using CraftNet.Services;

namespace Demo;

public delegate void SendToClientAction(ushort opcode, uint? rpcId, object body);

public class Session : Entity
{
    private readonly SendToClientAction _sendToClient;

    public readonly uint Id;

    /// <summary>
    /// 客户端的IP地址
    /// </summary>
    public IPEndPoint ClientAddress { get; private set; }

    /// <summary>
    /// 与Session相关的ActorId
    /// </summary>
    public readonly IActor Actor;

    // 用于加密
    public XRandom InRandom  = new XRandom(0);
    public XRandom OutRandom = new XRandom(0);

    public Session(uint id, IActor actor, IPEndPoint clientAddress, SendToClientAction sendToClient)
    {
        this.Id            = id;
        this.Actor         = actor;
        this._sendToClient = sendToClient;
        this.ClientAddress = clientAddress;
        this.Actor.Target  = this;
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
    public void Send(ushort opcode, IMessage message) => _sendToClient.Invoke(opcode, null, message);

    /// <summary>
    /// 给客户端回应一个消息(回应Request)
    /// </summary>
    /// <param name="opcode"></param>
    /// <param name="rpcId"></param>
    /// <param name="response"></param>
    public void Reply(ushort opcode, uint rpcId, IResponse response) => _sendToClient.Invoke(opcode, rpcId, response);
}