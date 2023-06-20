using System.Runtime.InteropServices;

namespace XGFramework.Services;

[StructLayout(LayoutKind.Explicit)]
public readonly struct ActorId
{
    [FieldOffset(0)] public readonly ushort PId; // 最多65535个进程
    [FieldOffset(2)] public readonly uint   Index;
    [FieldOffset(6)] public readonly byte   Type; // 类型0:普通 1:App类型
    [FieldOffset(0)] public readonly ulong  Value;

    public bool IsValid() => PId != 0 && Index != 0;

    public ActorId(ushort pid, uint index, byte type = 0)
    {
        this.Value = 0;
        this.PId   = pid;
        this.Index = index;
        this.Type  = type;
    }

    public ActorId(ulong id)
    {
        this.Value = id;
    }

    public override string ToString()
    {
        return $"{PId}[{Type}]-{Index:x2}";
    }

    /// <summary>
    /// 隐式转换
    /// </summary>
    /// <param name="id"></param>
    /// <returns></returns>
    public static implicit operator ActorId(ulong id) => new ActorId(id);

    public static implicit operator ulong(ActorId id) => id.Value;

    public static implicit operator ActorId(AppId id) => new ActorId(id.PId, id.Index, 1);
}