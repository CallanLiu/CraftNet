using System.Runtime.CompilerServices;

namespace CraftNet;

/// <summary>
/// 轻量级异步锁(线程安全)
/// </summary>
public class AsyncLock : ICriticalNotifyCompletion, IDisposable
{
    private Queue<Action> _queue = new();
    public bool IsCompleted => false;

    public AsyncLock GetResult()
    {
        return this;
    }

    public AsyncLock GetAwaiter() => this;

    public void OnCompleted(Action continuation)
    {
        Action action = null;
        lock (_queue)
        {
            if (_queue.Count == 0)
                action = continuation;
            _queue.Enqueue(continuation);
        }

        action?.Invoke();
    }

    public void UnsafeOnCompleted(Action continuation)
    {
        OnCompleted(continuation);
    }

    public void Dispose()
    {
        Action action;
        lock (_queue)
        {
            _queue.Dequeue();
            _queue.TryPeek(out action);
        }

        action?.Invoke();
    }
}