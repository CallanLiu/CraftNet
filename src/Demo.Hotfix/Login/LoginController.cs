using Microsoft.AspNetCore.Mvc;
using CraftNet;
using CraftNet.Services;

namespace Demo;

[ApiController]
[Route("[controller]/[action]")]
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

    /// <summary>
    /// 登录
    /// </summary>
    [HttpGet]
    public async Task<string> Login()
    {
        var       gateList      = _startConfig.GetAppConfigs(AppType.Gate);
        AppConfig gateAppConfig = gateList.Rand();

        Login2G_GetTokenResp resp =
            await _actorService.Call<Login2G_GetTokenResp>(gateAppConfig.ActorId, new Login2G_GetTokenReq());

        return "hello:" + resp.Token;
    }
}