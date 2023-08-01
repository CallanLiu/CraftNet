using System.Reflection;
using CraftNet.Services;
using Microsoft.Extensions.DependencyInjection;

namespace CraftNet;

/// <summary>
/// 消息系统实现
///     1. 发现其它服务端
/// </summary>
public class ActorSystem : IActorSystem
{
    private readonly IActorService   _actorService;
    private readonly ActorSystemComp _comp;
    private readonly App             _app;
    private          uint            _rpcIds;

    public ActorSystem(App app)
    {
        _app          = app;
        _actorService = app.Services.GetService<IActorService>();

        if (app.IsFirstLoad)
        {
            _comp = app.AddComponent<ActorSystemComp>();
        }

        _comp = app.GetComponent<ActorSystemComp>();
        _comp.MessageHandlers.Clear();
    }

    public void RegisterHandler<T>() where T : IMessageHandler
    {
        AlwaysInterleaveAttribute attr             = typeof(T).GetCustomAttribute<AlwaysInterleaveAttribute>();
        bool?                     alwaysInterleave = null;
        if (attr != null)
            alwaysInterleave = attr.IsTrue;
        _comp.MessageHandlers.Add(T.Opcode, (ActivatorUtilities.CreateInstance<T>(_app.Services), alwaysInterleave));
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

    public IActor GetActor(uint type, long key) => _actorService.Get(type, key);

    public void Send<TMessage>(ActorId id, TMessage msg) where TMessage : IMessage, IMessageMeta
    {
        // 使用消息服务进行投递
        ActorMessage actorMessage = ActorMessage.CreateMessage(id, TMessage.Opcode, msg);
        _actorService.Post(actorMessage);
    }

    public ValueTask<IResponse> Call<TReq>(ActorId id, TReq request) where TReq : IRequest, IMessageMeta
    {
        ResponseCompletionSource<IResponse> tcs = ResponseCompletionSourcePool.Get<IResponse>();
        ActorMessage actorMessage =
            ActorMessage.CreateRequest(id, TReq.Opcode, ++_rpcIds, request, tcs);
        _actorService.Post(actorMessage);
        return tcs.AsValueTask();
    }

    public IActor CreateActor(uint type, long key, bool isReentrant = false)
    {
        return _actorService.Create(type, key, _app.Scheduler, _comp, isReentrant);
    }

    public IActor CreateActor<TFilter>(uint type, long key, bool isReentrant = false)
        where TFilter : IMessageFilter
    {
        if (_comp.MessageFilters is null || _comp.MessageFilters.Length <= TFilter.Id ||
            _comp.MessageFilters[TFilter.Id] is null)
            throw new ArgumentException($"拦截器未注册: {typeof(TFilter).Name}", nameof(TFilter));
        return _actorService.Create(type, key, _app.Scheduler, _comp, isReentrant, TFilter.Id);
    }
}