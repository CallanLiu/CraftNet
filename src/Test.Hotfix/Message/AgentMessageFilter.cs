using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Test;
using XGFramework.Services;

namespace Test.Hotfix;

public class AgentMessageFilter : MessageFilter<Agent>
{
    private readonly IActorService _actorService;
    private readonly HashSet<ushort> _set;

    public AgentMessageFilter()
    {
        _actorService = App.Services.GetService<IActorService>();
        _set            = new HashSet<ushort>();
        _set.Add(PingReq.Opcode);
        _set.Add(TestWsMessage.Opcode);
    }

    protected override async ValueTask<bool> On(Agent self, MessageFilterContext context)
    {
        ActorMessage actorMessage = context.Context;
        if (actorMessage.Extra == 0) // 默认值0，内部处理。使用2表示内部想发往客户端的消息。
            return true;

        Console.WriteLine($"拦截Agent消息:{context.Context.Opcode}");

        // 外部消息: 
        //  1.内部发送给Agent想回给客户端的Message/Response
        //  2.收到客户端的Message/Request
        // 无法区分Message方向，需要个标记，表示消息方向。是来自客户端还是来自内部的Actor.

        if (actorMessage.Extra == 2)
        {
            self.Send(actorMessage.Opcode, (IMessage)actorMessage.Body);
            return false;
        }

        // Extra=1客户端发上来的消息
        if (_set.Contains(actorMessage.Opcode)) // Agent自己处理
        {
            object result = await context.Invoke();
            if (actorMessage.Type is MessageType.Request)
            {
                // 回给客户端
                IResponse response = (IResponse)result;
                Log.Debug("响应: {@response}", response);
                self.Reply(response.GetOpcode(), actorMessage.RpcId, response);
            }

            return false;
        }

        if (actorMessage.Type == MessageType.Message)
        {
            _actorService.Post(0, actorMessage.Type, actorMessage.Opcode, 0, actorMessage.Body);
        }
        else // IRequest
        {
            var tcs = ResponseCompletionSourcePool.Get<IResponse>();
            _actorService.Post(0, actorMessage.Type, actorMessage.Opcode, actorMessage.RpcId,
                actorMessage.Body, tcs);
            IResponse response = await tcs.AsValueTask();
            self.Reply(response.GetOpcode(), actorMessage.RpcId, response);
        }

        return false;
    }
}