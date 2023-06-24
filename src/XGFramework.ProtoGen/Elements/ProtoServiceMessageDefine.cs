using System.Text;

namespace XGFramework.ProtoGen;

/// <summary>
/// 服务中的消息
/// </summary>
public class ProtoServiceMessageDefine : ProtoMessageDefine
{
    public ProtoServiceMessageDefine(string line) : base(line)
    {

    }

    public override void ToString(StringBuilder sb, int tab)
    {
        // 判断是否存在Ack
        bool isRpc = false;
        foreach (var item in Elems)
        {
            if (item is ProtoMessageDefine { Name: "Ack" })
            {
                isRpc = true;
                break;
            }
        }



    }
}