using System.Buffers;

namespace CraftNet;

public static class LengthFieldBasedFrameDecoder
{
    /// <summary>
    /// 解析
    /// </summary>
    /// <param name="buffer"></param>
    /// <param name="bodyLength"></param>
    /// <param name="reader">返回数据部分(不包括长度字段)</param>
    /// <returns></returns>
    public static bool TryParse(ref ReadOnlySequence<byte> buffer, out uint bodyLength, out SequenceReader<byte> reader)
    {
        bodyLength = 0;
        reader     = new SequenceReader<byte>(buffer);
        if (reader.Remaining < sizeof(uint) || !reader.TryReadLittleEndian(out bodyLength))
            return false;

        if (reader.Remaining < bodyLength)
            return false;

        reader = new SequenceReader<byte>(reader.UnreadSequence.Slice(0, bodyLength));
        return true;
    }
}