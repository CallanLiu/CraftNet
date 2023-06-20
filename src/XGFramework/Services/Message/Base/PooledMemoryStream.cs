using Microsoft.IO;

namespace XGFramework.Services;

public static class PooledMemoryStream
{
    private static readonly RecyclableMemoryStreamManager _manager;

    static PooledMemoryStream()
    {
        // 4k
        int blockSize = 4096;

        // 大缓冲区倍数(每个大缓冲区将是该值的倍数) 1M
        // 线性增长: 1*m 2*m ... 直到maxBuffer最大值(8)将会有8个数组分别:1M,2M...8M
        int largeBufferMultiple = 1024 * 1024;

        // 大于此值的缓冲区将不缓存(8M)
        int maxBuffer = 8 * largeBufferMultiple;

        // 在将来丢弃缓冲区以进行垃圾回收之前，在小型池中保持可用的最大字节数.
        int maxSmallPoolFreeBytes = 0;

        // 在将来丢弃缓冲区以进行垃圾回收之前，大型池中保持可用的最大字节数.
        int maxLargePoolFreeBytes = 0;

        _manager = new RecyclableMemoryStreamManager(blockSize, largeBufferMultiple, maxBuffer, maxSmallPoolFreeBytes,
            maxLargePoolFreeBytes);
    }

    public static RecyclableMemoryStream Get() => (RecyclableMemoryStream)_manager.GetStream();
}