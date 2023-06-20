using Serilog;
using XGFramework;
using XGFramework.Services;

namespace Test.Hotfix;

public class PingAppReqHandler : RequestHandler<App, PingAppReq, PingAppResp>
{
    private int i = 0;

    protected override ValueTask On(App self, PingAppReq req, Reply reply)
    {
        ++i;
        if (i == TestSystem.LogCount)
        {
            i = 0;
            Log.Debug("{LogCount}次Req...", TestSystem.LogCount);
        }

        // Log.Debug("PingReq app: id={Id} name={Name}", self.Id, self.Name);
        reply.Invoke(new PingAppResp { Msg = DateTime.Now.ToString() });
        return ValueTask.CompletedTask;
    }
}