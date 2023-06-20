using Test;
using XGFramework.Services;

namespace Test.Hotfix;

public class PingHandler : RequestHandler<Agent, PingReq, PingResp>
{
    protected override ValueTask On(Agent self, PingReq req, Reply reply)
    {
        Console.WriteLine("收到ping...");
        reply.Invoke(new PingResp { Msg = "服务端时间:" + DateTime.Now.ToString() });
        return ValueTask.CompletedTask;
    }
}