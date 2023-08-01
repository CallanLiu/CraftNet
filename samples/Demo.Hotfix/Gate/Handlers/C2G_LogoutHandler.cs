using CraftNet.Services;

namespace Demo;

public class C2G_LogoutHandler : RequestHandler<Session, C2G_LogoutReq, C2G_LogoutResp>
{
    protected override ValueTask On(Session self, C2G_LogoutReq req, Reply reply)
    {
        C2G_LogoutResp resp = new C2G_LogoutResp();

        
        
        reply.Invoke(resp);
        return ValueTask.CompletedTask;
    }
}