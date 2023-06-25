using Microsoft.AspNetCore.Mvc;

namespace Demo;

[ApiController]
[Route("[controller]/[action]")]
public class LoginController : ControllerBase
{
    /// <summary>
    /// 登录
    /// </summary>
    [HttpGet]
    public string Login()
    {
        return "hello";
    }
}