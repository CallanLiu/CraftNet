namespace CraftNet
{
    /// <summary>
    /// 分配标记，记录索引与ChunkNode
    /// </summary>
    public ref struct AllocMark
    {
        public int Index;
        public BufferChunk Node;
    }
}