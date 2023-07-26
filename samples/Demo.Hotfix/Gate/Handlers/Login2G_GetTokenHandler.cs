using Serilog;
using CraftNet;
using CraftNet.Services;
using Microsoft.Extensions.Caching.Memory;

namespace Demo;

public class Login2G_GetTokenHandler : RequestHandler<App, Login2G_GetTokenReq, Login2G_GetTokenResp>
{
    private readonly IMemoryCache _memoryCache;

    public Login2G_GetTokenHandler(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    protected override ValueTask On(App self, Login2G_GetTokenReq req, Reply reply)
    {
        Log.Debug("[gate] 添加登录Token: {AccountId}, {Token}", req.AccountId, req.Token);
        _memoryCache.Set(req.Token, req.AccountId, TimeSpan.FromMinutes(1));
        reply.Invoke(new Login2G_GetTokenResp());
        return ValueTask.CompletedTask;
    }
}