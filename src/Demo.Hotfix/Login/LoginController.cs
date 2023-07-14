using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using CraftNet;
using CraftNet.Services;
using Demo.DB;
using Serilog;

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
    const            string        signKey = "^*)(&^#^&*";

    public LoginController(StartConfig startConfig, IActorService actorService)
    {
        _startConfig  = startConfig;
        _actorService = actorService;
    }

    [HttpGet(nameof(Test))]
    public string Test(string token)
    {
        byte[]    bytes     = Convert.FromBase64String(token);
        TokenInfo tokenInfo = JsonSerializer.Deserialize<TokenInfo>(bytes);
        bool      b         = tokenInfo.Check(signKey);
        return $"验证token结果: {b}";
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


        TokenInfo tokenInfo = new TokenInfo
        {
            Id       = account.Id,
            GateIp   = gateAppConfig.ProcessConfig.Urls,
            ExpireAt = (int)(DateTime.Now.AddMinutes(5).Ticks / TimeSpan.TicksPerMinute)
        };


        tokenInfo.Sign(signKey);
        string json = JsonSerializer.Serialize(tokenInfo);
        loginResp.Token = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
        Log.Debug(json);
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