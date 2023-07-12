namespace CraftNet.Services;

public struct MessageFilterContext
{
    public readonly  ActorMessage                       Context;
    private readonly Func<ActorMessage, ValueTask>      _invoke;
    private          IResponseCompletionSource<IResponse> _tmpTcs;

    public MessageFilterContext(ActorMessage context, Func<ActorMessage, ValueTask> invoke)
    {
        Context = context;
        _invoke = invoke;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="waitInvokeCompleted">等待整个handler方法调用完成，而不仅仅是Reply响应了。</param>
    /// <returns></returns>
    public async ValueTask<object> Invoke(bool waitInvokeCompleted = false)
    {
        // 如果是rpc需要获得
        if (this.Context.Type == MessageType.Request)
        {
            _tmpTcs = Context.Tcs;
            var tcs = ResponseCompletionSourcePool.Get<IResponse>();
            Context.Tcs = tcs;

            // 调用handler(不等待整个方法执行完成，只等待响应结果)
            ValueTask valueTask = _invoke(this.Context);
            if (waitInvokeCompleted)
                await valueTask;

            IResponse response = await tcs.AsValueTask();

            // 放回去，由拦截器自行处理。
            Context.Tcs = _tmpTcs;
            _tmpTcs     = null;

            // if (_tmpTcs != null) // 接着执行外部等待
            // {
            //     Context.RpcReply.Invoke(response, Context);
            // }

            return response;
        }

        await _invoke(this.Context);
        return null;
    }
}