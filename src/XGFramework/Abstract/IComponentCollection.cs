namespace XGFramework;

/// <summary>
/// 组件集合
/// </summary>
public interface IComponentCollection
{
    private static int componentIndex;

    public static class Id<T> where T : IComp
    {
        public static readonly int Value;

        static Id()
        {
            Value = componentIndex++;
        }
    }

    /// <summary>
    /// 获取组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T GetComp<T>() where T : IComp;

    /// <summary>
    /// 添加组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T AddComp<T>() where T : IComp, new();

    /// <summary>
    /// 移除组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    bool RemoveComp<T>() where T : IComp;
}