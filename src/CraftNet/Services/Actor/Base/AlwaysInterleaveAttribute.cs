namespace CraftNet.Services;

/// <summary>
/// 始终交错执行
/// </summary>
[AttributeUsage(AttributeTargets.Class, AllowMultiple = false, Inherited = false)]
public class AlwaysInterleaveAttribute : Attribute
{
    public readonly bool IsTrue;

    public AlwaysInterleaveAttribute()
    {
        IsTrue = true;
    }
    
    public AlwaysInterleaveAttribute(bool isTrue)
    {
        IsTrue = isTrue;
    }
}