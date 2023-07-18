using System.Threading.Tasks.Sources;

namespace CraftNet;

public class SingleWaiterAutoResetEvent : IValueTaskSource
{
    private ManualResetValueTaskSourceCore<bool> waitSource;

    private int hasWaiter = 1;

    public void GetResult(short token)
    {
        waitSource.GetResult(token);
        if (hasWaiter != 0) // 防止并发使用
            ThrowConcurrencyViolation();

        waitSource.Reset();
        Volatile.Write(ref hasWaiter, 1);
    }

    public ValueTaskSourceStatus GetStatus(short token) => waitSource.GetStatus(token);

    public void OnCompleted(Action<object> continuation, object state, short token,
        ValueTaskSourceOnCompletedFlags flags) => waitSource.OnCompleted(continuation, state, token, flags);

    public ValueTask WaitAsync() => new(this, waitSource.Version);

    public void Signal()
    {
        if (Interlocked.Exchange(ref hasWaiter, 0) == 1)
        {
            waitSource.SetResult(true);
        }
    }

    private static void ThrowConcurrencyViolation() =>
        throw new InvalidOperationException("Concurrent use is not supported");
}