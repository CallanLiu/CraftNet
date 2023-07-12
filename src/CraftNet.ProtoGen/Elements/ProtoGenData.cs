﻿
namespace CraftNet.ProtoGen;

public class ProtoGenData : Stack<ProtoElement>
{
    public string Namespace { get; set; }
    
    public List<string> UsingList { get; } = new(); // 自定义使用的命名空间

    public ushort Opcode { get; set; }

    public readonly List<ProtoElement> Elems = new();

    public ProtoElement TryPeek()
    {
        if (this.Count == 0)
            return null;
        return this.Peek();
    }
}