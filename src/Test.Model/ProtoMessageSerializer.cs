using System.Buffers;
using System.IO.Pipelines;
using ProtoBuf.Meta;
using XGFramework.Services;

namespace Test;

public class ProtoMessageSerializer : IMessageSerializer
{
    public object Deserialize(Type type, ReadOnlySequence<byte> buffer)
    {
        if (buffer.Length == 0)
            return Activator.CreateInstance(type);
        return RuntimeTypeModel.Default.Deserialize(type, buffer);
    }

    public void Serialize(object obj, PipeWriter pipeWriter)
    {
        RuntimeTypeModel.Default.Serialize(pipeWriter, obj);
    }
}