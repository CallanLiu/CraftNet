using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace XGFramework;

/// <summary>
/// 
/// </summary>
public interface IAppScheduler
{
    /// <summary>
    /// 用来切换到当前调度器上下文中
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Size = 1)]
    public readonly struct YieldAwaitable
    {
        private readonly IAppScheduler _scheduler;

        public YieldAwaitable(IAppScheduler scheduler)
        {
            _scheduler = scheduler;
        }

        public YieldAwaiter GetAwaiter() => new(_scheduler);

        [StructLayout(LayoutKind.Sequential, Size = 1)]
        public readonly struct YieldAwaiter : ICriticalNotifyCompletion
        {
            private readonly IAppScheduler _scheduler;

            public bool IsCompleted => false;

            public void GetResult()
            {
            }

            public YieldAwaiter(IAppScheduler scheduler)
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