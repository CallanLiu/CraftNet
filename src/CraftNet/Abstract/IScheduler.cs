using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace CraftNet;

/// <summary>
/// 
/// </summary>
public interface IScheduler
{
    /// <summary>
    /// 用来切换到当前调度器上下文中
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public readonly struct YieldAwaitable
    {
        private readonly IScheduler _scheduler;

        public YieldAwaitable(IScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public YieldAwaiter GetAwaiter() => new(_scheduler);

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public readonly struct YieldAwaiter : ICriticalNotifyCompletion
        {
            private readonly IScheduler _scheduler;

            public bool IsCompleted => false;

            public void GetResult()
            {
            }

            public YieldAwaiter(IScheduler scheduler)
            {
                _scheduler = scheduler;
            }

            public void OnCompleted(Action continuation)
            {
                _scheduler.Execute(continuation);
            }

            public void UnsafeOnCompleted(Action continuation)
            {
                OnCompleted(continuation);
            }
        }
    }

    void Execute(Task task);
    void Execute(Action workItem);
    void Execute(Action<object> workItem, object state);
    void Execute(IThreadPoolWorkItem workItem);

    YieldAwaitable Yield();
}