namespace CraftNet;

/// <summary>
/// 组件集合
/// </summary>
public interface IStateCollection
{
    private static int Index;

    public static class Id<T>
    {
        public static readonly int Value;

        static Id()
        {
            Value = Index++;
        }
    }

    /// <summary>
    /// 获取组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T GetState<T>();

    /// <summary>
    /// 添加组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <returns></returns>
    T AddState<T>() where T : new();

    T AddState<T>(T ins);

    /// <summary>
    /// 移除组件
    /// </summary>
    /// <typeparam name="T"></typeparam>
    bool RemoveState<T>();
}