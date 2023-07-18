using System;
using System.IO;
using System.Threading;

namespace CraftNet
{
    public partial class MemoryBuffer : Stream
    {
        private readonly int chunkSize; // 每个块的大小

        private BufferChunk headNode;
        private BufferChunk tailNode;

        private volatile int length;

        private int? markReadLength; // 标记读取长度(用来保证反序列化不会一次性多读)

        public int LastIndex { get; private set; }

        public int FirstIndex { get; private set; }

        public override bool CanRead => true;

        public override bool CanSeek => true;

        public override bool CanWrite => true;

        /// <summary>
        /// 流的长度
        /// </summary>
        public override long Length => length;

        /// <summary>
        /// 流的读取位置。
        /// 根据当前Chunk，读完一个ChunkSize后Position自动重置为0.
        /// </summary> 
        public override long Position
        {
            get => FirstIndex;
            set => FirstIndex = (int)value;
        }

        public byte[] LastBuffer => tailNode.buffer;

        /// <summary>
        /// 尾部Chunk可写长度
        /// </summary>
        public int TailChunkWriteableBytes => this.chunkSize - this.LastIndex;


        /// <summary>
        /// 头部Chunk可读长度
        /// </summary>

        public int HeadChunkReadableBytes =>
            Math.Min(this.markReadLength ?? this.length, this.chunkSize - this.FirstIndex);

        public byte[] First => this.headNode.buffer;

        public override long Seek(long offset, SeekOrigin origin)
        {
            if (offset < 0 || origin != SeekOrigin.Current)
                throw new NotSupportedException();

            // 有效数据不够
            if (offset > this.Length)
                throw new ArgumentOutOfRangeException();

            int offsetTmp = (int)offset;
            while (offsetTmp != 0)
            {
                int len = Math.Min(this.chunkSize - this.FirstIndex, offsetTmp);
                offsetTmp -= len;
                this.ReadAdvance(len);
            }

            return offset;
        }

        public override void SetLength(long value)
        {
            throw new NotImplementedException();
        }

        public override void Flush()
        {
        }

        public MemoryBuffer(int chunkSize = 8192)
        {
            this.chunkSize = chunkSize;

            // 初始化第一块
            this.headNode      = new BufferChunk(this.chunkSize);
            this.tailNode      = this.headNode;
            this.headNode.Next = this.tailNode;
        }

        private void RemoveFirst()
        {
            BufferChunk bufferChunk = this.headNode;
            this.headNode = headNode.Next;
            bufferChunk.Dispose();
        }

        private void AddLast()
        {
            this.tailNode = this.tailNode.Next = new BufferChunk(this.chunkSize);
        }

        protected override void Dispose(bool disposing)
        {
            base.Dispose(disposing);
            while (headNode != null)
            {
                RemoveFirst();
            }

            this.headNode = null;
            this.tailNode = null;
        }

        #region 读取相关

        public void MarkReadLength(int? value) => this.markReadLength = value;

        public override int Read(byte[] buffer, int offset, int count)
        {
            int readCount = 0;

            // 保证不超过可读的长度
            int limitCount = Math.Min(this.markReadLength ?? this.length, count);

            while (readCount < limitCount)
            {
                // 剩余多少没读的
                int n = limitCount - readCount;

                // 可读的
                n = Math.Min(HeadChunkReadableBytes, n);

                Array.Copy(this.First, this.FirstIndex, buffer, readCount + offset, n);
                readCount += n;
                this.ReadAdvance(n);
            }

            return readCount;
        }

        public override int ReadByte()
        {
            int limitCount = this.markReadLength ?? this.length;
            if (limitCount == 0)
                throw new IndexOutOfRangeException();

            int val = this.First[this.FirstIndex];
            this.ReadAdvance(1);
            return val;
        }

        /// <summary>
        /// 内部使用，请勿自行调用。
        /// 必须保证读取数量吻合ChunkSize
        /// </summary>
        /// <param name="count"></param>
        public void ReadAdvance(int count)
        {
            this.FirstIndex += count;
            if (this.FirstIndex == this.chunkSize)
            {
                this.RemoveFirst();
                this.FirstIndex = 0;
            }

            if (this.markReadLength != null)
                this.markReadLength -= count;

            // 保证逻辑都正确了，才改变长度
            Interlocked.Add(ref length, -count);
        }

        #endregion

        #region 写入相关

        /// <summary>
        /// 使用标记
        /// </summary>
        public AllocMark GetWriteMark()
        {
            AllocMark mark = new AllocMark
            {
                Index = this.LastIndex,
                Node  = this.tailNode
            };
            return mark;
        }

        public override void Write(byte[] buffer, int offset, int count)
        {
            int writeCount = 0;
            while (writeCount < count)
            {
                // 剩余多少还没写入
                int n = count - writeCount;

                // 限制写入不能超过buffer剩余的长度.
                n = Math.Min(this.TailChunkWriteableBytes, n);

                Array.Copy(buffer, writeCount + offset, this.LastBuffer, this.LastIndex, n);

                writeCount += n;

                this.WriteAdvance(n);
            }
        }

        public override void WriteByte(byte value)
        {
            LastBuffer[LastIndex] = value;
            this.WriteAdvance(1);
        }

        /// <summary>
        /// 内部使用，请勿自行调用。
        /// 必须保证写入数量吻合ChunkSize
        /// </summary>
        /// <param name="count"></param>
        public void WriteAdvance(int count)
        {
            this.LastIndex += count;
            if (this.LastIndex == this.chunkSize)
            {
                this.AddLast();
                this.LastIndex = 0;
            }

            // 保证buffer都添加好了，才改变长度。
            Interlocked.Add(ref length, count);
        }

        #endregion


        public override string ToString()
        {
            return $"r:{this.FirstIndex} w:{this.LastIndex} l:{this.length}";
        }
    }
}