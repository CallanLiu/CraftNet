using Microsoft.AspNetCore.Mvc;
using CraftNet;
using CraftNet.Services;
using Demo.DB;

namespace Demo;

[ApiController]
[Route("[controller]")]
public class LoginController : ControllerBase
{
    /*
     * 此示例通过静态配置，获取Gate服。
     * 想要动态获取可用类【注册中心】相关的中间件
     */
    private readonly StartConfig   _startConfig;
    private readonly IActorService _actorService;

    public LoginController(StartConfig startConfig, IActorService actorService)
    {
        _startConfig  = startConfig;
        _actorService = actorService;
    }

    [HttpGet(nameof(Test))]
    public string Test(string token)
    {
        return "hello";
    }

    /// <summary>
    /// 登录
    /// </summary>
    [HttpPost]
    public async Task<LoginResp> Login(LoginReq req)
    {
        LoginResp loginResp = new LoginResp();

        DBAccount account = await DBAccount.LoginFind(req.Username, req.Password, DateTime.Now, "");
        if (account is null)
        {
            // 账号不存在
            loginResp.Err = Err.Login_AccountNotExist;
            return loginResp;
        }

        var       gateList      = _startConfig.GetAppConfigs(AppType.Gate);
        AppConfig gateAppConfig = gateList.Rand();

        // 生成一个token
        string token = Guid.NewGuid().ToString("N");
        Login2G_GetTokenResp login2GGetTokenResp = await _actorService.Call<Login2G_GetTokenResp>(gateAppConfig.ActorId,
            new Login2G_GetTokenReq
            {
                AccountId = account.Id,
                Token     = token
            });

        if (login2GGetTokenResp.Err != 0)
        {
            loginResp.Err = login2GGetTokenResp.Err;
        }
        else
        {
            loginResp.Token = token;
        }

        return loginResp;
    }

    /// <summary>
    /// 注册
    /// </summary>
    [HttpPost(nameof(Register))]
    public async Task Register(RegisterReq req)
    {
    }
}