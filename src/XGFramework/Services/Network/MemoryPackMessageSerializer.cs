using System.Buffers;
using System.IO.Pipelines;
using MemoryPack;
using XGFramework.Services;

namespace XGFramework;

public class MemoryPackMessageSerializer : IMessageSerializer
{
    public object Deserialize(Type type, ReadOnlySequence<byte> buffer)
    {
        return MemoryPackSerializer.Deserialize(type, buffer);
    }

    public void Serialize(object obj, PipeWriter pipeWriter)
    {
        MemoryPackSerializer.Serialize(obj.GetType(), pipeWriter, obj);
    }
}