using System.Text;

namespace XGFramework.ProtoGen;

public class ProtoMessageDefine : ProtoElementDefine
{
    public string Name { get; }
    public string ParentClass { get; }

    public ProtoMessageDefine(string line)
    {
        Name = line.Split(Proto2CS.splitChars, StringSplitOptions.RemoveEmptyEntries)[1].Trim();
        string[] ss = line.Split(new[] { "//:" }, StringSplitOptions.RemoveEmptyEntries);
        if (ss is { Length: > 1 })
        {
            ParentClass = ss[1].Trim();
        }
    }

    public override void ToString(StringBuilder sb, int tab)
    {
        string className = Name;
        string baseClass = ParentClass;
        if (Parent is ProtoMessageDefine && className == "Ack")
        {
            baseClass = "IResponse";
        }

        string tabStr = "";
        for (int i = 0; i < tab; i++)
        {
            tabStr += "\t";
        }

        sb.Append(tabStr);
        sb.AppendLine("[ProtoContract]");

        sb.Append(tabStr);
        sb.Append($"public partial class {className}");
        if (!string.IsNullOrEmpty(baseClass))
        {
            sb.Append(" : ");
            sb.Append(baseClass);
        }

        sb.AppendLine();

        sb.Append(tabStr);
        sb.AppendLine("{");
        foreach (var item in Elems)
        {
            item.ToString(sb, tab + 1);
        }

        sb.Append(tabStr);
        sb.AppendLine("}");
        sb.AppendLine();
    }
}