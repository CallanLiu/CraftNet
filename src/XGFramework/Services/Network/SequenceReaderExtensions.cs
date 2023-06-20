using System.Buffers;

namespace XGFramework;

public static class SequenceReaderExtensions
{
    public static bool TryReadBigEndian(this ref SequenceReader<byte> reader, out ushort value)
    {
        bool b = reader.TryReadBigEndian(out short v);
        value = (ushort)v;
        return b;
    }

    public static bool TryReadBigEndian(this ref SequenceReader<byte> reader, out uint value)
    {
        bool b = reader.TryReadBigEndian(out int v);
        value = (uint)v;
        return b;
    }

    public static bool TryReadBigEndian(this ref SequenceReader<byte> reader, out ulong value)
    {
        bool b = reader.TryReadBigEndian(out long v);
        value = (ulong)v;
        return b;
    }
}