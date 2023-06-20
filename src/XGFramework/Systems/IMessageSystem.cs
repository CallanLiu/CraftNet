using XGFramework.Services;

namespace XGFramework;

/// <summary>
/// 消息系统(服务之间通信)
/// </summary>
public interface IMessageSystem : ISystemBase<IMessageSystem>, ISystemTypeId
{
    void RegisterHandler<T>() where T : IMessageHandler, new();

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
    /// 发送消息
    /// </summary>
    /// <param name="id"></param>
    /// <param name="msg"></param>
    void Send<TMessage>(ActorId id, TMessage msg) where TMessage : IMessage, IMessageMeta;

    ValueTask<IResponse> Call<TReq>(ActorId id, TReq request) where TReq : IRequest, IMessageMeta;
}