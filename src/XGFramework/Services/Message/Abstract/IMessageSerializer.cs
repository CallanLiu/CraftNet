using System.Buffers;
using System.IO.Pipelines;

namespace XGFramework.Services;

/// <summary>
/// 消息序列化
/// </summary>
public interface IMessageSerializer
{
    object Deserialize(Type type, ReadOnlySequence<byte> buffer);

    void Serialize(object obj, PipeWriter pipeWriter);
}