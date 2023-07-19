namespace Demo.DB;

/// <summary>
/// 账户db操作
/// </summary>
public class DBAccount
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

    /// <summary>
    /// 感觉用户名和密码进行登录查询
    /// </summary>
    /// <param name="username"></param>
    /// <param name="password"></param>
    /// <param name="loginTime"></param>
    /// <param name="loginIp"></param>
    /// <returns></returns>
    public static async ValueTask<DBAccount> LoginFind(string username, string password, DateTime loginTime,
        string loginIp)
    {
        return new DBAccount();
    }
}