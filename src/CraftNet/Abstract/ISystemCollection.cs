namespace CraftNet;

public interface ISystemCollection
{
    // 系统不会在运行过程中减少/增加类型
    private static int systemTypeIndexValue = 0;

    internal static class Index<T> where T : ISystemTypeId
    {
        public static readonly int Value;

        /// <summary>
        /// 多线程使用安全(静态构造是线程安全的)
        /// </summary>
        static Index()
        {
            Value = systemTypeIndexValue++;
        }
    }

    IReadOnlyList<ISystemBase> AllSystems { get; }

    T AddSystem<T, TImpl>(uint group = 0) where T : ISystemBase where TImpl : T;

    T GetSystem<T>() where T : ISystemBase;
}