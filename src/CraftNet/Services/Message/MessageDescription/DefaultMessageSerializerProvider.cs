using System.Reflection;
using MemoryPack;

namespace CraftNet.Services;

/// <summary>
/// 默认提供MemoryPack序列化
/// </summary>
public class DefaultMessageSerializerProvider : IMessageSerializerProvider
{
    public readonly MemoryPackMessageSerializer MemoryPackMessageSerializer = new MemoryPackMessageSerializer();

    public virtual IMessageSerializer Get(Type type, ushort opcode)
    {
        if (type.GetCustomAttribute<MemoryPackableAttribute>() != null)
            return this.MemoryPackMessageSerializer;
        return null;
    }
}