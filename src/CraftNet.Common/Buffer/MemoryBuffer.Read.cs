using System;

namespace CraftNet
{
    public partial class MemoryBuffer
    {
        public long ReadLong() => (long)this.ReadULong();

        public ulong ReadULong()
        {
            ulong value = 0;
            value |= (byte)this.ReadByte();
            value |= (ulong)this.ReadByte() << 8;
            value |= (ulong)this.ReadByte() << 16;
            value |= (ulong)this.ReadByte() << 24;
            value |= (ulong)this.ReadByte() << 32;
            value |= (ulong)this.ReadByte() << 40;
            value |= (ulong)this.ReadByte() << 48;
            value |= (ulong)this.ReadByte() << 56;
            return value;
        }

        public int ReadInt() => (int)ReadUInt();

        public uint ReadUInt()
        {
            uint value = 0;
            value |= (uint)this.ReadByte();
            value |= (uint)(this.ReadByte() << 8);
            value |= (uint)(this.ReadByte() << 16);
            value |= (uint)(this.ReadByte() << 24);
            return value;
        }

        public ushort ReadUShort()
        {
            ushort value = 0;
            value |= (ushort)this.ReadByte();
            value |= (ushort)(this.ReadByte() << 8);
            return value;
        }

        /// <summary>
        /// 读取一个int, 但不推进索引
        /// </summary>
        /// <returns></returns>
        public uint ReadUIntWithoutAdvance()
        {
            int         readIndex = this.FirstIndex;
            BufferChunk readNode  = this.headNode;
            uint        value     = 0;
            value |= this.ReadByteWithoutAdvance(ref readIndex, ref readNode);
            value |= (uint)(this.ReadByteWithoutAdvance(ref readIndex, ref readNode) << 8);
            value |= (uint)(this.ReadByteWithoutAdvance(ref readIndex, ref readNode) << 16);
            value |= (uint)(this.ReadByteWithoutAdvance(ref readIndex, ref readNode) << 24);
            return value;
        }

        public int ReadIntWithoutAdvance() => (int)ReadUIntWithoutAdvance();

        /// <summary>
        /// 读取一个byte，但不推进索引
        /// </summary>
        /// <returns></returns>
        private byte ReadByteWithoutAdvance(ref int readIndex, ref BufferChunk node)
        {
            if (this.length == 0)
                throw new IndexOutOfRangeException();

            byte val = node.buffer[readIndex];

            readIndex += 1;
            if (readIndex == this.chunkSize)
            {
                node      = node.Next;
                readIndex = 0;
            }

            return val;
        }
    }
}