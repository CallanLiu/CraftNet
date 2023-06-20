using Microsoft.Extensions.DependencyInjection;
using XGFramework.Services;

namespace XGFramework;

/// <summary>
/// 消息系统实现
///     1. 发现其它服务端
/// </summary>
public class MessageSystem : IMessageSystem
{
    private readonly IActorService   _actorService;
    private readonly MessageSystemComp _comp;
    private readonly App               _app;
    private          uint              _rpcIds;

    public MessageSystem(App app)
    {
        _app            = app;
        _actorService = app.Services.GetService<IActorService>();

        if (app.IsFirstLoad)
        {
            _comp = app.AddComp<MessageSystemComp>();
        }

        _comp = app.GetComp<MessageSystemComp>();
        _comp.Handlers.Clear();
    }

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

    public void RegisterHandler<T>() where T : IMessageHandler, new()
    {
        _comp.Handlers.Add(T.Opcode, new T());
    }

    public void RegisterFilter<T>() where T : IMessageFilter, new()
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
        _comp.MessageFilters[T.Id] ??= new T();
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