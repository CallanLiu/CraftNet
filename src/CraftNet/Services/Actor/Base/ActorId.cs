namespace CraftNet.Services;

/// <summary>
/// 类型化的ActorId
/// </summary>
public readonly struct ActorId : IEquatable<ActorId>, IComparable<ActorId>
{
    /// <summary>
    /// 进程Id
    /// </summary>
    public readonly ushort PId;

    /// <summary>
    /// Actor类型
    /// </summary>
    public readonly uint Type;

    /// <summary>
    /// 唯一Key(不同类型的Key可以一样)
    /// </summary>
    public readonly long Key;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="pid">进程id</param>
    /// <param name="type">actor类型</param>
    /// <param name="key"></param>
    public ActorId(ushort pid, uint type, long key)
    {
        this.PId  = pid;
        this.Key  = key;
        this.Type = type;
    }

    public override string ToString() => $"{PId}/{Type:x2}/{Key}";

    public bool Equals(ActorId other)
    {
        return PId == other.PId && Key == other.Key && Type == other.Type;
    }

    public override bool Equals(object obj)
    {
        return obj is ActorId other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(PId, Key, Type);
    }

    public int CompareTo(ActorId other)
    {
        var pIdComparison = PId.CompareTo(other.PId);
        if (pIdComparison != 0) return pIdComparison;
        var keyComparison = Key.CompareTo(other.Key);
        if (keyComparison != 0) return keyComparison;
        return Type.CompareTo(other.Type);
    }
}