using CraftNet.Services;
using Microsoft.Extensions.Caching.Memory;
using Serilog;

namespace Demo;

public partial class C2G_LoginGateHandler : RequestHandler<Session, C2G_LoginGateReq, C2G_LoginGateResp>
{
    private readonly IMemoryCache _memoryCache;

    public C2G_LoginGateHandler(IMemoryCache memoryCache)
    {
        _memoryCache = memoryCache;
    }

    protected override ValueTask On(Session self, C2G_LoginGateReq req, Reply reply)
    {
        C2G_LoginGateResp resp = new C2G_LoginGateResp();

        if (string.IsNullOrEmpty(req.Token))
        {
            resp.Err = Err.Login_AccountNotExist;
            reply.Invoke(resp);
            return ValueTask.CompletedTask;
        }

        if (!_memoryCache.TryGetValue(req.Token, out int accountId))
        {
            resp.Err = 1;
        }
        else
        {
            _memoryCache.Remove(req.Token);
            Log.Debug("[gate] 玩家登录网关: accountId={AccountId}, token={Token}", accountId, req.Token);
        }

        reply.Invoke(resp);
        return ValueTask.CompletedTask;
    }
}