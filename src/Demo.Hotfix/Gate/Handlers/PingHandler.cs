using CraftNet.Services;

namespace Demo;

public class PingHandler : RequestHandler<Agent, C2G_PingReq, C2G_PingResp>
{
    protected override ValueTask On(Agent self, C2G_PingReq req, Reply reply)
    {
        Console.WriteLine("收到ping...");
        reply.Invoke(new C2G_PingResp { });
        return ValueTask.CompletedTask;
    }
}