using System.Text;

namespace XGFramework.ProtoGen;

public class ProtoServiceDefine : ProtoElementDefine
{
    public readonly string Name;

    public List<string> Attriubtes = new List<string>();

    public ProtoServiceDefine(string line)
    {
        // "//~"
        string[] temp = line.Substring(3).Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
        Name = temp[0];

        // 特性
        for (int i = 1; i < temp.Length; i++)
        {
            Attriubtes.Add(temp[i]);
        }
    }

    public override void ToString(StringBuilder sb, int tab)
    {
        // 先生成协议消息
        foreach (var item in Elems)
        {
            item.ToString(sb, tab);
        }

        //sb.AppendLine($"public ");
    }
}