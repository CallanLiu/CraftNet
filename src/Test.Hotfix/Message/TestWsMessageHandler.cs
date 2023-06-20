using Serilog;
using Test;
using XGFramework.Services;

namespace Test.Hotfix;

public class TestWsMessageHandler : MessageHandler<Agent, TestWsMessage>
{
    protected override ValueTask On(Agent self, TestWsMessage message)
    {
        Log.Debug("Agent: {Txt}", message.Text);
        return ValueTask.CompletedTask;
    }
}