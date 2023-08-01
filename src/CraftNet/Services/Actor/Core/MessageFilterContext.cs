using System.Reflection;

namespace CraftNet.Services;

public struct MessageFilterContext
{
    public readonly  ActorMessage                         Context;
    private readonly IMessageHandler                      _messageHandler;
    private          IResponseCompletionSource<IResponse> _tmpTcs;

    public MessageFilterContext(ActorMessage context, IMessageHandler messageHandler)
    {
        Context         = context;
        _messageHandler = messageHandler;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="waitInvokeCompleted">等待整个handler方法调用完成，而不仅仅是Reply响应了。</param>
    /// <returns></returns>
    public async ValueTask<object> Invoke(bool waitInvokeCompleted = false)
    {
        if (_messageHandler is null)
            throw new TargetException($"MessageHandler不存在,无法进行调用: Opcode={this.Context.Opcode}");

        // 如果是rpc需要获得
        if (this.Context.Type == MessageType.Request)
        {
            _tmpTcs = Context.Tcs;
            var tcs = ResponseCompletionSourcePool.Get<IResponse>();
            Context.Tcs = tcs;

            // 调用handler(不等待整个方法执行完成，只等待响应结果)
            ValueTask valueTask = _messageHandler.Invoke(this.Context);
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

        await _messageHandler.Invoke(this.Context);
        return null;
    }
}