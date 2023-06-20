namespace XGFramework;

public interface ISetup
{
    public bool IsFirst { get; }

    public App App { get; }

    /// <summary>
    /// 注册系统的Setup
    /// </summary>
    /// <param name="self"></param>
    /// <typeparam name="T"></typeparam>
    void RegisterSetup<T>(T self) where T : ISystemSetup;

    /// <summary>
    /// 获取系统Setup
    /// </summary>
    /// <typeparam name="T"></typeparam>
    T GetSetup<T>() where T : ISystemSetup;

    /// <summary>
    /// 检测是否再Enter状态中，如果不在就抛出异常。
    /// </summary>
    void CheckThrowException();
}