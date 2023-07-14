using CraftNet.Services;

namespace CraftNet;

/// <summary>
/// 消息系统(服务之间通信)
/// </summary>
public interface IActorSystem : ISystemBase<IActorSystem>, ISystemTypeId
{
    /// <summary>
    /// 注册消息处理器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    void RegisterHandler<T>() where T : IMessageHandler, new();

    /// <summary>
    /// 注册消息拦截器
    /// </summary>
    /// <typeparam name="T"></typeparam>
    void RegisterFilter<T>() where T : IMessageFilter, new();

    /// <summary>
    /// 成为Actor
    /// </summary>
    /// <param name="target"></param>
    /// <param name="isReentrant">是否消息重入</param>
    /// <returns></returns>
    ActorId CreateActor(object target, bool isReentrant = false);

    /// <summary>
    /// 成为Actor并使指定一个拦截器
    /// </summary>
    /// <param name="target"></param>
    /// <param name="isReentrant"></param>
    /// <typeparam name="TFilter"></typeparam>
    /// <returns></returns>
    ActorId CreateActor<TFilter>(object target, bool isReentrant = false) where TFilter : IMessageFilter;

    /// <summary>
    /// 获取Actor消息队列
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    ActorMailbox GetActorMailbox(ActorId id);

    /// <summary>
    /// 发送消息
    /// </summary>
    /// <param name="id"></param>
    /// <param name="msg"></param>
    void Send<TMessage>(ActorId id, TMessage msg) where TMessage : IMessage, IMessageMeta;

    /// <summary>
    /// Rpc远程消息
    /// </summary>
    /// <param name="id"></param>
    /// <param name="request"></param>
    /// <typeparam name="TReq"></typeparam>
    /// <returns></returns>
    ValueTask<IResponse> Call<TReq>(ActorId id, TReq request) where TReq : IRequest, IMessageMeta;
}