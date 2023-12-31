﻿using System.Runtime.InteropServices;

namespace CraftNet;

[StructLayout(LayoutKind.Explicit)]
public readonly struct AppId
{
    [FieldOffset(0)] public readonly uint   Index;
    [FieldOffset(6)] public readonly ushort PId; // 最多65535个进程
    [FieldOffset(0)] public readonly uint   Value;

    public bool IsValid() => PId != 0;

    public AppId(ushort pid, ushort index)
    {
        this.Value = 0;
        this.PId   = pid;
        this.Index = index;
    }

    public AppId(uint id)
    {
        this.PId   = 0;
        this.Index = 0;
        this.Value = id;
    }

    public override string ToString()
    {
        return $"{PId}-{Index:x2}";
    }
}