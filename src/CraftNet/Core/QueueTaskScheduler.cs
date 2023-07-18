using System.Runtime.CompilerServices;

namespace CraftNet;

/// <summary>
/// 队列化的任务调度器
/// </summary>
public class QueueTaskScheduler : TaskScheduler, IThreadPoolWorkItem
{
    private enum Status
    {
        Waiting  = 0,
        Runnable = 1,
        Running  = 2
    }

    private readonly   Queue<Task> _workItems = new();
    private readonly   object      _syncObj   = new();
    private            Status      _state;
    protected readonly App         Context;

    public QueueTaskScheduler(App context)
    {
        this.Context = context;
    }

    protected override IEnumerable<Task> GetScheduledTasks()
    {
        return _workItems;
    }

    private void RunTask(Task task)
    {
        RuntimeContext.Set(Context);
        bool done = TryExecuteTask(task);
        if (!done)
            throw new Exception($"task done false: {Context}");

        RuntimeContext.Reset();
    }

    public void EnqueueTask(Task task)
    {
        lock (_syncObj)
        {
            _workItems.Enqueue(task);
            if (_state != Status.Waiting)
                return;

            _state = Status.Runnable;
            ScheduleExecution(this);
        }
    }

    protected override void QueueTask(Task task) => EnqueueTask(task);

    /// <summary>
    /// 只要在同个上下文，才有机会进行内联执行。
    /// </summary>
    /// <param name="task"></param>
    /// <param name="taskWasPreviouslyQueued"></param>
    /// <returns></returns>
    protected override bool TryExecuteTaskInline(Task task, bool taskWasPreviouslyQueued)
    {
        var  currentContext   = RuntimeContext.Current;
        bool canExecuteInline = currentContext != null && object.Equals(currentContext, Context);
        if (!canExecuteInline)
            return false;

        // 如果任务以前已排队，将其从队列中删除
        if (taskWasPreviouslyQueued)
            canExecuteInline = TryDequeue(task);

        if (!canExecuteInline)
            return false;

        return TryExecuteTask(task);
    }

    public void Execute()
    {
        try
        {
            do
            {
                lock (_syncObj)
                    _state = Status.Running;

                Task task;
                lock (_syncObj)
                {
                    if (_workItems.Count > 0)
                        task = _workItems.Dequeue();
                    else
                        break;
                }

                RunTask(task);
            } while (true);
        }
        finally
        {
            lock (_syncObj)
            {
                if (_workItems.Count > 0)
                {
                    _state = Status.Runnable;
                    ScheduleExecution(this);
                }
                else
                {
                    _state = Status.Waiting;
                }
            }
        }
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static void ScheduleExecution(IThreadPoolWorkItem workItem)
    {
        ThreadPool.UnsafeQueueUserWorkItem(workItem, preferLocal: true);
    }
}