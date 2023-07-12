using Serilog;
using CraftNet;
using CraftNet.Services;

namespace Demo;

public class Login2G_GetTokenHandler : RequestHandler<App, Login2G_GetTokenReq, Login2G_GetTokenResp>
{
    protected override ValueTask On(App self, Login2G_GetTokenReq req, Reply reply)
    {
        Log.Debug("Login2G...");

        reply.Invoke(new Login2G_GetTokenResp
        {
            Token = Random.Shared.Next()
        });
        return ValueTask.CompletedTask;
    }
}