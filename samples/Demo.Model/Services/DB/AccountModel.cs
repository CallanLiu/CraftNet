namespace Demo.DB;

/// <summary>
/// 账号
/// </summary>
public class AccountModel
{
    /// <summary>
    /// 唯一id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 用户名
    /// </summary>
    public string Username { get; set; }

    /// <summary>
    /// 密码
    /// </summary>
    public string Password { get; set; }

    /// <summary>
    /// 注册时间
    /// </summary>
    public DateTime RegisterAt { get; set; }

    /// <summary>
    /// 最近登录时间
    /// </summary>
    public DateTime LastLoginTime { get; set; }

    /// <summary>
    /// 最近登录ip
    /// </summary>
    public string LastLoginIp { get; set; }
}