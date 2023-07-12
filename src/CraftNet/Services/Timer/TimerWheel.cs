using System.Collections.Concurrent;
using System.Diagnostics;

namespace CraftNet.Services;

/// <summary>
/// 时间轮
/// </summary>
public partial class TimerWheel : ITimerService, IThreadPoolWorkItem
{
    private const uint UnitMs         = 10;
    private const long TickIntervalMs = UnitMs * TimeSpan.TicksPerMillisecond;

    private const uint Mask1 = 0xFF;
    private const uint Mask2 = 0x3F;

    // 阈值
    private const uint L1Threshold = Mask1 + 1;
    private const uint L2Threshold = L1Threshold << 6;
    private const uint L3Threshold = L2Threshold << 6;
    private const uint L4Threshold = L3Threshold << 6;

    /// <summary>
    /// 线程同步使用
    /// </summary>
    struct Cmd
    {
        public bool      IsAdd;
        public TimerNode Node;
    }

    // 5层
    private readonly TimerSlot       _tv1;
    private readonly TimerSlot       _tv2;
    private readonly TimerSlot       _tv3;
    private readonly TimerSlot       _tv4;
    private readonly TimerSlot       _tv5;
    private readonly TimerLinkedList _freeTimers;

    // 当前时间
    private          uint                 _curIndex = 0;
    private readonly Timer                _timer;
    private readonly ConcurrentQueue<Cmd> _cmdQueue = new();
    private readonly Action<TimerNode>    _disposableAction;
    private          int                  _doingWork;

    private long _lastTime;
    private long _tickTimeNum;

    public TimerWheel()
    {
        _disposableAction = Remove;
        _tv1              = new TimerSlot(1, 256, Mask1, 0, this);
        _tv2              = new TimerSlot(2, 64, Mask2, 8, this);
        _tv3              = new TimerSlot(3, 64, Mask2, 14, this);
        _tv4              = new TimerSlot(4, 64, Mask2, 20, this);
        _tv5              = new TimerSlot(5, 64, Mask2, 26, this);
        _freeTimers       = new TimerLinkedList(this);
        _timer            = new Timer(this.OnTimeout, this, 0, UnitMs);
        _lastTime         = Stopwatch.GetTimestamp();
    }

    public ITimer AddTimeout(int type, uint timeout, TimerCallback callback, object state = null) =>
        InternalAdd(timeout, callback, state, type);

    public ITimer AddInterval(int type, uint interval, TimerCallback callback, object state = null) =>
        InternalAdd(interval, callback, state, type, interval);

    private ITimer InternalAdd(uint timeout, TimerCallback callback, object state, int type,
        uint interval = 0)
    {
        timeout  /= UnitMs;
        interval /= UnitMs;
        TimerNode timer = new TimerNode(timeout, state, _disposableAction, type, interval, callback);
        _cmdQueue.Enqueue(new Cmd { IsAdd = true, Node = timer });
        return timer;
    }

    private void InternalAdd(TimerNode t)
    {
        // 绝对超时时间
        ulong expires = t.Expires = _curIndex + t.Expires;

        TimerSlot slot;
        if (expires < L1Threshold) slot      = _tv1;
        else if (expires < L2Threshold) slot = _tv2;
        else if (expires < L3Threshold) slot = _tv3;
        else if (expires < L4Threshold) slot = _tv4;
        else
        {
            if (expires > uint.MaxValue) // 超过最大值放入特殊列表；[优化]减掉现在到最大值后,从头放入?
            {
                _freeTimers.AddLast(t);
                return;
            }

            slot = _tv5;
        }

        slot.Add(t);
    }

    private void Remove(TimerNode t) => _cmdQueue.Enqueue(new Cmd { IsAdd = false, Node = t });

    private void OnTimeout(object state)
    {
        if (Interlocked.CompareExchange(ref _doingWork, 1, 0) == 0)
            ThreadPool.UnsafeQueueUserWorkItem(this, true);
    }

    static bool TryCascade(uint time, TimerSlot cur, TimerSlot next)
    {
        byte nextIndex = next.GetIndex(time);
        next.MoveTo(nextIndex, cur);
        return false;
    }

    /// <summary>
    /// 使用 System.Threading.Timer 进行超时,每次超时后马上使用线程池调用一次Execute以执行TimerWheel的逻辑。
    /// </summary>
    public void Execute()
    {
        long curTime = Stopwatch.GetTimestamp();
        long delta   = curTime - _lastTime;
        _lastTime = curTime;

        _tickTimeNum += delta;
        while (_tickTimeNum >= TickIntervalMs)
        {
            Tick();
            _tickTimeNum -= TickIntervalMs;
        }

        Interlocked.Exchange(ref _doingWork, 0);
    }

    private void Tick()
    {
        // 处理计时器操作
        while (_cmdQueue.TryDequeue(out Cmd cmd))
        {
            if (cmd.IsAdd)
                InternalAdd(cmd.Node);
            else
                cmd.Node.List.Remove(cmd.Node);
        }

        // 尝试级联
        byte index = (byte)_curIndex;
        TryCascade(_curIndex, _tv4, _tv5);
        TryCascade(_curIndex, _tv3, _tv4);
        TryCascade(_curIndex, _tv2, _tv3);
        TryCascade(_curIndex, _tv1, _tv2);
        Trigger(index);

        unchecked
        {
            ++_curIndex;
        }

        // 溢出归零后，超过最大时间的定时器全部重新放入一遍。
        if (_curIndex == 0)
        {
            TimerNode t = _freeTimers.First;
            while (t != null)
            {
                t.Expires -= uint.MaxValue;
                t.List.Remove(t);
                InternalAdd(t); // 重新放入
                t = t.Next;
            }
        }
    }

    private void Trigger(byte index)
    {
        TimerLinkedList list = _tv1.Timers[index];
        while (list.First != null)
        {
            TimerNode node = list.First;
            node.Invoke();
            list.Remove(node);

            if (node.Interval > 0)
            {
                // 重新添加
                node.Expires = node.Interval;
                InternalAdd(node);
            }
        }
    }
}