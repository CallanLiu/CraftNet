using System.Text;

namespace XGFramework.ProtoGen;

public abstract class ProtoElementDefine
{
    public List<ProtoElementDefine> Elems = new List<ProtoElementDefine>();

    public ProtoElementDefine Parent { get; private set; }

    public abstract void ToString(StringBuilder sb, int tab);

    public void AddChild(ProtoElementDefine child)
    {
        Elems.Add(child);
        child.Parent = this;
    }
}