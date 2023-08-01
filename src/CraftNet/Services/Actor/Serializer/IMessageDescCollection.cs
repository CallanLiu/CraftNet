using System.Reflection;

namespace CraftNet.Services;

public interface IMessageDescCollection
{
    void Add(Assembly assembly);
    void Add<T>() where T : IMessageBase, IMessageMeta;
    void Add(Type type, ushort opcode);
    bool TryGet(Type type, out MessageDesc desc);
    bool TryGet(ushort opcode, out MessageDesc desc);
}