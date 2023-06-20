namespace XGFramework.Services;

public interface IMessageSender
{
    public ushort PId { get; }

    /// <summary>
    /// 发送一个消息
    /// </summary>
    /// <param name="message"></param>
    void Send(ActorMessage message);

    /// <summary>
    /// 收到响应
    /// </summary>
    /// <param name="message"></param>
    void OnResponse(ActorMessage message);
}