using System.Collections;
using System.Runtime.CompilerServices;

namespace CraftNet;

/// <summary>
/// 基于插槽的集合
/// </summary>
public class Slots<T> : IEnumerable<T>
{
    struct KV
    {
        public uint handle;
        public T    value;
    }

    private readonly int maxSize;

    private KV?[] arr;
    private uint  handleIndex = 1;

    public int Count { get; private set; }

    public Slots(int initSize = 4, int maxSize = ushort.MaxValue)
    {
        this.maxSize = maxSize;
        arr          = new KV?[initSize];
    }

    public T this[uint handle]
    {
        get
        {
            uint hash = Handle2Hash(handle);
            KV?  kv   = arr[hash];
            if (kv == null)
            {
                throw new KeyNotFoundException($"找不到Handle: {handle}, hash={hash}");
            }

            return kv.Value.value;
        }
        set
        {
            uint hash = Handle2Hash(handle);
            KV?  kv   = arr[hash];
            if (kv == null)
            {
                throw new KeyNotFoundException($"找不到Handle: {handle}, hash={hash}");
            }

            arr[hash] = new KV
            {
                handle = handle,
                value  = value
            };
        }
    }

    public bool TryGet(uint handle, out T t)
    {
        KV? kv = arr[Handle2Hash(handle)];
        if (kv == null)
        {
            t = default;
            return false;
        }

        t = kv.Value.value;
        return true;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="handle"></param>
    /// <param name="value"></param>
    /// <exception cref="Exception"></exception>
    public void Add(uint handle, T value)
    {
        uint hash = Handle2Hash(handle);
        do
        {
            if (!arr[hash].HasValue)
            {
                arr[hash] = new KV()
                {
                    handle = handle,
                    value  = value
                };
                ++Count;
                break;
            }

            // 调整大小
            if (!Resize())
                throw new Exception($"已大最大容量: {handle}, hash={hash}");

            hash = Handle2Hash(handle); // 重新计算一下
        } while (true);
    }

    /// <summary>
    /// 调整大小
    /// </summary>
    private bool Resize()
    {
        if (arr.Length > maxSize) return false;

        // 全部找完都没有空位，就扩容, 并根据handle放入新的位置。
        KV?[] newArr = new KV?[arr.Length * 2];
        foreach (var kv in arr)
        {
            if (!kv.HasValue) continue;
            uint rehash = kv.Value.handle & ((uint)newArr.Length - 1);
            newArr[rehash] = kv;
        }

        arr = newArr;
        return true;
    }

    /// <summary>
    /// 分配一个
    /// </summary>
    /// <returns></returns>
    // public uint? Alloc()
    // {
    //     
    // }
    public bool TryAdd(T value, out uint handle)
    {
        while (true)
        {
            handle = handleIndex;
            for (uint i = 0, len = (uint)arr.Length; i < len; ++i, ++handle)
            {
                if (handle > maxSize)
                    handle = 1;

                uint hash = handle & (len - 1);
                KV?  kv   = arr[hash];
                if (kv == null)
                {
                    handleIndex = handle + 1;
                    arr[hash] = new KV
                    {
                        handle = handle,
                        value  = value
                    };
                    ++Count;
                    return true;
                }
            }

            if (!Resize())
                return false;
        }
    }

    public bool Remove(uint handle)
    {
        uint hash  = Handle2Hash(handle);
        KV?  value = arr[hash];
        if (value.HasValue)
        {
            arr[hash] = null;
            --Count;
            return true;
        }

        return false;
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private uint Handle2Hash(uint handle) => handle & ((uint)arr.Length - 1);

    public void Clear()
    {
        for (int i = 0; i < arr.Length; i++)
            arr[i] = default;
        Count = 0;
    }

    public IEnumerator<T> GetEnumerator()
    {
        foreach (KV? value in arr)
        {
            if (value != null)
                yield return value.Value.value;
        }
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}