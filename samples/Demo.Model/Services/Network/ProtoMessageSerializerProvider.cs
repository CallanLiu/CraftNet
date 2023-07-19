using System.Buffers;
using System.IO.Pipelines;
using System.Reflection;
using CraftNet.Services;
using ProtoBuf;
using ProtoBuf.Meta;

namespace Demo.Services.Network;

public class ProtoMessageSerializerProvider : DefaultMessageSerializerProvider, IMessageSerializer
{
    public override IMessageSerializer Get(Type type, ushort opcode)
    {
        IMessageSerializer messageSerializer = base.Get(type, opcode);
        if (messageSerializer is null)
        {
            if (type.GetCustomAttribute<ProtoContractAttribute>() != null)
            {
                return this;
            }
        }

        return null;
    }

    public object Deserialize(Type type, ReadOnlySequence<byte> buffer)
    {
        return RuntimeTypeModel.Default.Deserialize(type, buffer);
    }

    public void Serialize(object obj, PipeWriter pipeWriter)
    {
        RuntimeTypeModel.Default.Serialize(pipeWriter, obj);
    }
}