using System.Buffers;
using System.IO.Pipelines;
using System.Text;

namespace CraftNet.Services;

/// <summary>
/// 异常响应
/// </summary>
[ExceptionResponseSerializer]
public partial class ExceptionResponse : Exception, IResponse
{
    public ushort GetOpcode() => 0;

    public ExceptionResponse(string msg) : base(msg)
    {
    }
}

public class ExceptionResponseSerializer : MessageSerializerAttribute
{
    public override object Deserialize(Type type, ReadOnlySequence<byte> buffer)
    {
        return Encoding.UTF8.GetString(buffer);
    }

    public override void Serialize(object obj, PipeWriter pipeWriter)
    {
        Encoding.UTF8.GetBytes(obj.ToString(), pipeWriter);
    }
}