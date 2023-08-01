namespace CraftNet.Services;

public interface IMessageSerializerProvider
{
    IMessageSerializer Get(Type type, ushort opcode);
}