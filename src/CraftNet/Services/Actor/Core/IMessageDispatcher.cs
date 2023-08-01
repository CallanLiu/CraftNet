namespace CraftNet.Services;

/// <summary>
/// Actor分发接口
/// </summary>
public interface IMessageDispatcher
{
    IMessageFilter GetFilter(int filterId);
    (object, bool?) GetMessageHandler(ushort opcode);
}