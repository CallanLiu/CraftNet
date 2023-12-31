using Microsoft.Extensions.DependencyInjection;
using Serilog;
using CraftNet.Services;

namespace Demo;

public class SessionMessageFilter : MessageFilter<Session>
{
    private readonly IActorService   _actorService;
    private readonly HashSet<ushort> _set;

    public SessionMessageFilter()
    {
        _actorService = App.Services.GetService<IActorService>();
        _set = new HashSet<ushort>
        {
            C2G_PingReq.Opcode
        };
    }

    private bool IsOuterMessage(ushort opcode)
    {
        return opcode >= 10000;
    }

    protected override async ValueTask<bool> On(Session self, MessageFilterContext context)
    {
        ActorMessage actorMessage = context.Context;
        if (actorMessage.Direction == 0) // 默认值0，内部处理。
        {
            if (!IsOuterMessage(actorMessage.Opcode)) return true;

            // 内部发来的外部消息，全部发给客户端。
            if (actorMessage.Type == MessageType.Message)
            {
                self.Send(actorMessage.Opcode, (IMessage)actorMessage.Body);
            }
            else
            {
                self.Reply(actorMessage.Opcode, actorMessage.RpcId, (IResponse)actorMessage.Body);
            }

            return false;
        }

        Console.WriteLine($"拦截消息:{context.Context.Opcode}");

        // 外部消息: 
        //  1.收到客户端的Message/Request
        //  2.内部发送给Session想回给客户端的Message/Response
        // 无法区分Message方向，需要个标记，表示消息方向。是来自客户端还是来自内部的Actor.

        if (actorMessage.Direction == 1) // Extra=1客户端发上来的消息
        {
            // Session自己处理
            if (actorMessage.Opcode < 20000) // 都是网关处理的消息
            {
                object result = await context.Invoke();
                if (actorMessage.Type is MessageType.Request)
                {
                    // 回给客户端
                    IResponse response = (IResponse)result;
                    Log.Debug("响应: {@Response}", response);
                    self.Reply(response.GetOpcode(), actorMessage.RpcId, response);
                }

                return false;
            }

            // 转发
            if (actorMessage.Type == MessageType.Message)
            {
                _actorService.Post(ActorMessage.CreateMessage(new ActorId(), actorMessage.Opcode, actorMessage.Body));
            }
            else // IRequest
            {
                var tcs = ResponseCompletionSourcePool.Get<IResponse>();
                _actorService.Post(ActorMessage.CreateRequest(new ActorId(), actorMessage.Opcode, actorMessage.RpcId,
                    actorMessage.Body, tcs));
                IResponse response = await tcs.AsValueTask();
                self.Reply(response.GetOpcode(), actorMessage.RpcId, response);
            }
        }

        return false;
    }
}