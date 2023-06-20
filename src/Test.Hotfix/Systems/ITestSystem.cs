using XGFramework;

namespace Test.Hotfix;

public interface ITestSystem : ISystemBase<IIDTestSystem>
{
    void Test();
}