using System;
using System.Buffers;

namespace CraftNet
{
    public class BufferChunk : IDisposable
    {
        public byte[] buffer;

        // 后一个
        public volatile BufferChunk Next;

        public BufferChunk(int size)
        {
            this.buffer = ArrayPool<byte>.Shared.Rent(size);
        }

        public void Dispose()
        {
            if (buffer != null)
            {
                ArrayPool<byte>.Shared.Return(buffer);
            }

            Next   = null;
            buffer = null;
        }
    }
}