using Serilog;
using XGFramework;

namespace Demo;

public class OnWebSocketConnectedListener : EventListener<EventWebSocketConnected>
{
    public override void On(EventWebSocketConnected e)
    {
        Log.Debug("WebSocket connected...");

        Agent agent = e.Agent;

        // 让agent成为一个actor并且消息可重入
        IMessageSystem messageSystem = this.App.GetSystem<IMessageSystem>();
        agent.ActorId = messageSystem.CreateActor<AgentMessageFilter>(agent, true);
    }
}