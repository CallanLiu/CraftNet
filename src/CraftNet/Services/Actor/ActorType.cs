using System.IO.Hashing;
using System.Text;

namespace CraftNet.Services;

public static class ActorType<T>
{
    public static readonly uint Value;

    static unsafe ActorType()
    {
        uint hash;
        XxHash32.TryHash(Encoding.UTF8.GetBytes(typeof(T).FullName!), new Span<byte>((byte*)&hash, sizeof(uint)), out _);
        Value = hash;
    }
}