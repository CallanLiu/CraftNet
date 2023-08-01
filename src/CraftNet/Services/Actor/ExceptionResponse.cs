using System.Buffers;
using System.IO.Pipelines;
using System.Runtime.Serialization.Formatters.Binary;
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
        string msg = Encoding.UTF8.GetString(buffer);
        return new ExceptionResponse(msg);
    }

    public override void Serialize(object obj, PipeWriter pipeWriter)
    {
        Console.WriteLine(obj.ToString());
        Encoding.UTF8.GetBytes(obj.ToString(), pipeWriter);
    }
}