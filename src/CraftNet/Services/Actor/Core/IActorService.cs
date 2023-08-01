namespace CraftNet.Services;

/// <summary>
/// 消息服务
/// </summary>
public interface IActorService : IRpcReply
{
    /// <summary>
    /// 投递一个消息
    /// </summary>
    /// <param name="message"></param>
    void Post(ActorMessage message);

    /// <summary>
    /// 创建一个Actor(如果已经存在将引发异常)
    /// </summary>
    /// <param name="type"></param>
    /// <param name="key"></param>
    /// <param name="scheduler"></param>
    /// <param name="messageDispatcher"></param>
    /// <param name="isReentrant"></param>
    /// <param name="filterId"></param>
    /// <returns></returns>
    IActor Create(uint type, long key, IScheduler scheduler, IMessageDispatcher messageDispatcher,
        bool isReentrant = false, int? filterId = null);

    /// <summary>
    /// 获取一个Actor
    /// </summary>
    /// <param name="type"></param>
    /// <param name="key"></param>
    /// <returns></returns>
    IActor Get(uint type, long key);

    /// <summary>
    /// 销毁一个Actor
    /// </summary>
    /// <param name="actorId"></param>
    internal void Destroy(ActorId actorId);
}