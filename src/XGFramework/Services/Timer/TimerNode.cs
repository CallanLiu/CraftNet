namespace XGFramework.Services;

internal class TimerNode : ITimer
{
    private readonly TimerCallback _callback;

    public readonly uint Interval;

    public  ulong             Expires; // 最终超时时间
    private Action<TimerNode> _disposableAction;
    public  TimerNode         Prev, Next;
    public  TimerLinkedList   List;

    public int Type { get; }
    public object State { get; }

    public TimerNode(ulong expires, object state, Action<TimerNode> disposableAction, int type, uint interval,
        TimerCallback callback)
    {
        this.Expires           = expires;
        this.State             = state;
        this._disposableAction = disposableAction;
        this.Type              = type;
        this.Interval          = interval;
        this._callback         = callback;
    }

    public void Invoke() => _callback(this);

    /// <summary>
    /// 会在其它线程进行操作
    /// </summary>
    public void Dispose()
    {
        var tmp = Interlocked.Exchange(ref _disposableAction, null);
        tmp?.Invoke(this);
    }
}