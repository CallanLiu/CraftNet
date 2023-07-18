using System.IO;

namespace CraftNet
{
    public partial class MemoryBuffer
    {
        public void WriteLong(long value) => WriteULong((ulong)value);

        public void WriteULong(ulong value)
        {
            this.WriteByte((byte)value);
            this.WriteByte((byte)(value >> 8));
            this.WriteByte((byte)(value >> 16));
            this.WriteByte((byte)(value >> 24));
            this.WriteByte((byte)(value >> 32));
            this.WriteByte((byte)(value >> 40));
            this.WriteByte((byte)(value >> 48));
            this.WriteByte((byte)(value >> 56));
        }

        public void WriteInt(int value) => WriteUInt((uint)value);

        public void WriteUInt(uint value)
        {
            this.WriteByte((byte)value);
            this.WriteByte((byte)(value >> 8));
            this.WriteByte((byte)(value >> 16));
            this.WriteByte((byte)(value >> 24));
        }

        public void WriteUShort(ushort value)
        {
            this.WriteByte((byte)value);
            this.WriteByte((byte)(value >> 8));
        }

        public void WriteByteWithMark(ref AllocMark mark, byte value)
        {
            mark.Node.buffer[mark.Index] = value;
            ++mark.Index;
            if (mark.Index == this.chunkSize)
            {
                mark.Node  = mark.Node.Next;
                mark.Index = 0;
            }
        }

        public void WriteIntWithMark(ref AllocMark mark, int value) => WriteUIntWithMark(ref mark, (uint)value);

        public void WriteUIntWithMark(ref AllocMark mark, uint value)
        {
            this.WriteByteWithMark(ref mark, (byte)value);
            this.WriteByteWithMark(ref mark, (byte)(value >> 8));
            this.WriteByteWithMark(ref mark, (byte)(value >> 16));
            this.WriteByteWithMark(ref mark, (byte)(value >> 24));
        }

        public void Write(Stream stream)
        {
            int count = (int)(stream.Length - stream.Position);

            int alreadyCopyCount = 0;
            while (alreadyCopyCount < count)
            {
                int tailWriteableBytes = this.TailChunkWriteableBytes;
                int n                  = count - alreadyCopyCount;

                // 一次写完
                if (tailWriteableBytes > n)
                {
                    stream.Read(this.LastBuffer, this.LastIndex, n);
                    WriteAdvance(n);
                    alreadyCopyCount += n;
                }
                else
                {
                    stream.Read(this.LastBuffer, this.LastIndex, tailWriteableBytes);
                    WriteAdvance(tailWriteableBytes);
                    alreadyCopyCount += tailWriteableBytes;
                }
            }
        }
    }
}