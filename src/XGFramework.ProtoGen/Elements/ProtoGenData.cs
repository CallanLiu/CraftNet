using System.Text;

namespace XGFramework.ProtoGen;

public class ProtoGenData : Stack<ProtoElementDefine>
{
    public string Namespace { get; set; }
    
    public List<string> UsingList { get; } = new(); // 自定义使用的命名空间

    public ushort Opcode { get; set; }

    public readonly List<ProtoElementDefine> Elems = new();

    public ProtoElementDefine TryPeek()
    {
        if (this.Count == 0)
            return null;
        return this.Peek();
    }

    public StringBuilder Gen()
    {
        StringBuilder sb = new StringBuilder();

        foreach (var item in UsingList)
        {
            sb.AppendLine($"{item}");
        }

        sb.AppendLine($"namespace {Namespace};");
        sb.AppendLine();

        foreach (var item in Elems)
        {
            item.ToString(sb, 0);
        }

        return sb;
    }
}