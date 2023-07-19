using System.Security.Cryptography;
using System.Text;

namespace Demo;

public class TokenInfo
{
    /// <summary>
    /// 唯一Id
    /// </summary>
    public int Id { get; set; }

    /// <summary>
    /// 分配的网关Ip地址
    /// </summary>
    public string GateIp { get; set; }

    /// <summary>
    /// 过期时间
    /// </summary>
    public int ExpireAt { get; set; }

    /// <summary>
    /// 签名
    /// </summary>
    public string Signature { get; set; }

    /// <summary>
    /// 进行签名
    /// </summary>
    public void Sign(string key)
    {
        string secret = this.Id + this.GateIp + this.ExpireAt;
        this.Signature = GetHash(key, secret);
    }

    private static string GetHash(string key, string secret)
    {
        byte[]        bytes   = HMACSHA256.HashData(Encoding.ASCII.GetBytes(key), Encoding.ASCII.GetBytes(secret));
        StringBuilder builder = new StringBuilder();
        for (int i = 0; i < bytes.Length; i++)
            builder.AppendFormat("{0:x2}", bytes[i]);
        return builder.ToString();
    }

    /// <summary>
    /// 验证签名
    /// </summary>
    public bool Check(string key, bool checkExpired = true)
    {
        string secret = Id + GateIp + ExpireAt;
        string sign   = GetHash(key, secret);
        if (Signature != sign)
            return false;

        if (checkExpired)
        {
            var ts = TimeSpan.FromMinutes(ExpireAt);

            if (ts.Ticks < DateTime.Now.Ticks)
                return false;
        }

        return true;
    }
}