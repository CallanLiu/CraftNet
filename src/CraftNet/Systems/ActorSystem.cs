using CraftNet.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CraftNet;

/// <summary>
/// 消息系统实现
///     1. 发现其它服务端
/// </summary>
public class ActorSystem : IActorSystem
{
    private readonly IActorService          _actorService;
    private readonly ActorMessageSystemComp _comp;
    private readonly App                    _app;
    private          uint                   _rpcIds;

    public ActorSystem(App app)
    {
        _app          = app;
        _actorService = app.Services.GetService<IActorService>();

        if (app.IsFirstLoad)
        {
            _comp = app.AddComponent<ActorMessageSystemComp>();
        }

        _comp = app.GetComponent<ActorMessageSystemComp>();
        _comp.MessageHandlers.Clear();
    }

    public void RegisterHandler<T>() where T : IMessageHandler
    {
        _comp.MessageHandlers.Add(T.Opcode, ActivatorUtilities.CreateInstance<T>(_app.Services));
    }

    public void RegisterFilter<T>() where T : IMessageFilter
    {
        if (_comp.MessageFilters is null)
        {
            _comp.MessageFilters = new IMessageFilter[T.Id + 1];
        }
        else if (_comp.MessageFilters.Length <= T.Id)
        {
            var arr = _comp.MessageFilters;
            _comp.MessageFilters = new IMessageFilter[T.Id + 1];
            for (int i = 0; i < arr.Length; i++)
                _comp.MessageFilters[i] = arr[i];
        }

        // 如果不存在就创建
        _comp.MessageFilters[T.Id] ??= ActivatorUtilities.CreateInstance<T>(_app.Services);
    }

    public ActorMailbox GetActorMailbox(ActorId id) => _actorService.GetMailbox(id);

    public void Send<TMessage>(ActorId id, TMessage msg) where TMessage : IMessage, IMessageMeta
    {
        // 使用消息服务进行投递
        _actorService.Post(id, MessageType.Message, TMessage.Opcode, 0, msg, null);
    }

    public ValueTask<IResponse> Call<TReq>(ActorId id, TReq request) where TReq : IRequest, IMessageMeta
    {
        ResponseCompletionSource<IResponse> tcs = ResponseCompletionSourcePool.Get<IResponse>();
        _actorService.Post(id, MessageType.Request, TReq.Opcode, ++_rpcIds, request, tcs);
        return tcs.AsValueTask();
    }

    public ActorId CreateActor(object target, bool isReentrant = false)
    {
        return _actorService.Create(target, _app.Scheduler, _comp, isReentrant);
    }

    public ActorId CreateActor<TFilter>(object target, bool isReentrant = false) where TFilter : IMessageFilter
    {
        if (_comp.MessageFilters is null || _comp.MessageFilters.Length <= TFilter.Id ||
            _comp.MessageFilters[TFilter.Id] is null)
            throw new ArgumentException($"拦截器未注册: {typeof(TFilter).Name}", nameof(TFilter));
        return _actorService.Create(target, _app.Scheduler, _comp, isReentrant, TFilter.Id);
    }
}