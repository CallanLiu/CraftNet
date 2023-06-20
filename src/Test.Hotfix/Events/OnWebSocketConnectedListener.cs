using Serilog;
using Test;
using XGFramework;

namespace Test.Hotfix;

public class OnWebSocketConnectedListener : EventListener<EventWebSocketConnected>
{
    public override void On(EventWebSocketConnected e)
    {
        Log.Debug("WebSocket connected...");

        Agent agent = e.Agent;

        // 让agent成为一个actor并且消息可重入
        agent.ActorId = this.App.GetSystem<IMessageSystem>().CreateActor<AgentMessageFilter>(agent, true);
    }
}