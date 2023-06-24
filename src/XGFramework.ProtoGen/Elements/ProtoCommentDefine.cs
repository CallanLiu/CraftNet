using System.Text;

namespace XGFramework.ProtoGen;

public class ProtoCommentDefine : ProtoElementDefine
{
    public string Comment;
    public int    TabCount;

    public override void ToString(StringBuilder sb, int tab)
    {
        sb.AppendLine(Comment);
    }
}