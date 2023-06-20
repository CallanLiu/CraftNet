using System.Runtime.CompilerServices;

namespace XGFramework;

/// <summary>
/// 轻量级异步锁(线程不安全)
///     需在线程安全的环境下使用.
/// </summary>
public class AsyncLockUnsafe : IDisposable
{
    private readonly Queue<Action> _queue = new();

    public readonly struct AsyncLockAwaiter : ICriticalNotifyCompletion
    {
        private readonly AsyncLockUnsafe _asyncLockUnsafe;
        public bool IsCompleted => false;

        public AsyncLockUnsafe GetResult() => _asyncLockUnsafe;

        public AsyncLockAwaiter(AsyncLockUnsafe asyncLockUnsafe) => _asyncLockUnsafe = asyncLockUnsafe;

        public void OnCompleted(Action continuation)
        {
            Action action = null;
            if (_asyncLockUnsafe._queue.Count == 0)
                action = continuation;
            _asyncLockUnsafe._queue.Enqueue(continuation);
            action?.Invoke();
        }

        public void UnsafeOnCompleted(Action continuation)
        {
            OnCompleted(continuation);
        }
    }

    public AsyncLockAwaiter GetAwaiter() => new(this);

    /// <summary>
    /// 每次Dispose都将释放一次锁
    /// </summary>
    public void Dispose()
    {
        // 先丢掉当前的，并执行下一个。
        _queue.Dequeue();
        if (_queue.TryPeek(out var action))
            action?.Invoke();
    }
}