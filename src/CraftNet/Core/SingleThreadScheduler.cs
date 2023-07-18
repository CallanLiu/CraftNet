namespace CraftNet;

/// <summary>
/// 单线程的调度器
/// </summary>
public class SingleThreadScheduler : QueueTaskScheduler, IScheduler
{
    private readonly SingleWaiterAutoResetEvent _workSignal = new();
    private readonly Queue<WorkItem>            _works      = new();

    record struct WorkItem
    {
        public readonly  object Callback;
        public readonly  object State;
        private readonly byte   _type; // 0.action 1.action<object> 2.IThreadPoolWorkItem

        public WorkItem(IThreadPoolWorkItem a)
        {
            Callback = a;
            _type    = 2;
        }

        public WorkItem(Action a)
        {
            Callback = a;
            _type    = 0;
        }

        public WorkItem(Action<object> a, object state)
        {
            Callback   = a;
            this.State = state;
            _type      = 1;
        }

        public void Invoke()
        {
            switch (_type)
            {
                case 0:
                    ((Action)Callback).Invoke();
                    break;
                case 1:
                    ((Action<object>)Callback).Invoke(State);
                    break;
                case 2:
                    ((IThreadPoolWorkItem)Callback).Execute();
                    break;
            }
        }
    }

    public SingleThreadScheduler(App context) : base(context)
    {
        Task task = new Task(() => _ = RunLoop());
        this.Execute(task);
    }

    public void Execute(Task task) => task.Start(this);

    public void Execute(Action workItem)
    {
        lock (this)
        {
            _works.Enqueue(new WorkItem(workItem));
            _workSignal.Signal();
        }
    }

    public void Execute(Action<object> workItem, object state)
    {
        lock (this)
        {
            _works.Enqueue(new WorkItem(workItem, state));
            _workSignal.Signal();
        }
    }

    public void Execute(IThreadPoolWorkItem workItem)
    {
        lock (this)
        {
            _works.Enqueue(new WorkItem(workItem));
            _workSignal.Signal();
        }
    }

    /// <summary>
    /// 使用异步的方式，让其切换上下文。
    /// </summary>
    /// <returns></returns>
    public IScheduler.YieldAwaitable Yield() => new(this);

    /// <summary>
    /// 使用异步的循环
    /// </summary>
    private async Task RunLoop()
    {
        while (true)
        {
            do
            {
                WorkItem workItem;
                lock (this)
                {
                    if (!_works.TryDequeue(out workItem))
                        break;
                }

                try
                {
                    workItem.Invoke();
                }
                catch (Exception e)
                {
                    Console.WriteLine(e);
                    // this.schedulerContext.LogError(e);
                }
            } while (true);

            await _workSignal.WaitAsync();
        }
    }
}