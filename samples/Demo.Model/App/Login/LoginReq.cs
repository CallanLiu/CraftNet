namespace Demo;

public class LoginReq
{
    public string Username { get; set; }
    public string Password { get; set; }
}

public class LoginResp
{
    /// <summary>
    /// 登陆网关的令牌
    /// </summary>
    public string Token { get; set; }

    /// <summary>
    /// 网关ip
    /// </summary>
    public string GateIp { get; set; }

    /// <summary>
    /// 错误码
    /// </summary>
    public int Err { get; set; }
}