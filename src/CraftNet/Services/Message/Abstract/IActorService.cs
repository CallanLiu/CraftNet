﻿namespace CraftNet.Services;



/// <summary>
/// 消息服务
/// </summary>
public interface IActorService
{
    /// <summary>
    /// 投递一个消息(远程/本地)
    /// </summary>
    void Post(ActorId id, MessageType type, ushort opcode, uint rpcId, IMessageBase body,
        IResponseCompletionSource<IResponse> tcs = null, int extra = 0);

    /// <summary>
    /// 投递一个消息
    /// </summary>
    /// <param name="message"></param>
    void Post(ActorMessage message);

    /// <summary>
    /// 创建一个Actor
    /// </summary>
    /// <param name="target"></param>
    /// <param name="scheduler"></param>
    /// <param name="messageDispatcher"></param>
    /// <param name="isReentrant"></param>
    /// <param name="filterId"></param>
    /// <returns></returns>
    ActorId Create(object target, IScheduler scheduler, IActorMessageDispatcher messageDispatcher,
        bool isReentrant = false, int? filterId = null);

    ActorMailbox GetMailbox(ActorId actorId);
}