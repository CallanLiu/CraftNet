using XGFramework;
using XGFramework.Services;

namespace Test.Hotfix;

public class TestMessageHandler : MessageHandler<App, TestMessage>
{
    protected override ValueTask On(App self, TestMessage message)
    {
        Console.WriteLine($"收到TestMessage...");
        return ValueTask.CompletedTask;
    }
}