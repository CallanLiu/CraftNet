namespace CraftNet
{
    /// <summary>
    /// 固定循环队列
    /// </summary>
    public class FixedRingQueue<T>
    {
        private readonly int _bufferSize;
        private readonly T[] _buffer;
        private          int _readIndex  = 0;
        private          int _writeIndex = 0;

        /// <summary>
        /// 有效元素数量
        /// </summary>
        public int Count { get; private set; }

        /// <summary>
        /// 0最先入队的元素
        /// </summary>
        /// <param name="index"></param>
        public ref T this[int index] => ref _buffer[(_readIndex + index) % _bufferSize];

        /// <summary>
        /// 
        /// </summary>
        /// <param name="bufferSize">队列大小</param>
        public FixedRingQueue(int bufferSize = 16)
        {
            _buffer     = new T[bufferSize];
            _bufferSize = bufferSize;
        }

        /// <summary>
        /// 入队
        /// </summary>
        /// <param name="info"></param>
        public void Enqueue(T info)
        {
            _buffer[_writeIndex] = info;
            _writeIndex          = (_writeIndex + 1) % _bufferSize;

            // 满了, 覆盖最旧的一个,并移动读索引.
            if (Count == _bufferSize)
            {
                _readIndex = (_readIndex + 1) % _bufferSize;
            }
            else
            {
                ++Count;
            }
        }

        /// <summary>
        /// 出队
        /// </summary>
        public void Dequeue()
        {
            if (Count > 0)
            {
                _readIndex = (_readIndex + 1) % _bufferSize;
                --Count;
            }
        }

        /// <summary>
        /// 清空队列
        /// </summary>
        public void Clear()
        {
            _readIndex  = 0;
            _writeIndex = 0;
            Count       = 0;
        }
    }
}