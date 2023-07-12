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

    private            Queue<Task> workItems = new();
    private readonly   object      syncObj   = new();
    private            Status      state;
    protected readonly App         context;

    public QueueTaskScheduler(App context)
    {
        this.context = context;
    }

    protected override IEnumerable<Task> GetScheduledTasks()
    {
        return workItems;
    }

    private void RunTask(Task task)
    {
        RuntimeContext.Set(context);
        bool done = TryExecuteTask(task);
        if (!done)
            throw new Exception($"task done false: {context}");

        RuntimeContext.Reset();
    }

    public void EnqueueTask(Task task)
    {
        lock (syncObj)
        {
            workItems.Enqueue(task);
            if (state != Status.Waiting)
                return;

            state = Status.Runnable;
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
        bool canExecuteInline = currentContext != null && object.Equals(currentContext, context);
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
                lock (syncObj)
                    state = Status.Running;

                Task task;
                lock (syncObj)
                {
                    if (workItems.Count > 0)
                        task = workItems.Dequeue();
                    else
                        break;
                }

                RunTask(task);
            } while (true);
        }
        finally
        {
            lock (syncObj)
            {
                if (workItems.Count > 0)
                {
                    state = Status.Runnable;
                    ScheduleExecution(this);
                }
                else
                {
                    state = Status.Waiting;
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