namespace CraftNet;

public interface ICallback
{
    private static int _index;

    public static class Id<T>
    {
        public static readonly int Value;

        static Id()
        {
            Value = _index++;
        }
    }
}

public interface ICallback<TResult>
{
}

/// <summary>
/// 回调实现
/// </summary>
public interface ICallbackImpl<T>
{
    static abstract int Id { get; }
    void On(object target, in T e);
}

public interface ICallbackImpl<T, out TResult> : ICallbackImpl<T>
{
    new TResult On(object target, in T e);
}

public abstract class ActionImpl<T> : ICallbackImpl<T> where T : ICallback
{
    public static int Id => ICallback.Id<T>.Value;

    protected readonly App App;

    protected ActionImpl()
    {
        App = RuntimeContext.Current;
    }

    public void On(object target, in T e) => On(in e);

    protected abstract void On(in T e);
}

public abstract class FuncImpl<T, TResult> : ICallbackImpl<T, TResult> where T : ICallback<TResult>
{
    public static int Id => ICallback.Id<T>.Value;

    protected readonly App App;

    protected FuncImpl()
    {
        App = RuntimeContext.Current;
    }

    protected abstract TResult On(in T e);

    TResult ICallbackImpl<T, TResult>.On(object target, in T e)
    {
        return On(in e);
    }

    void ICallbackImpl<T>.On(object target, in T e)
    {
        throw new NotImplementedException();
    }
}

public abstract class ActionImpl<TSelf, T> : ICallbackImpl<T> where T : ICallback
{
    public static int Id => ICallback.Id<T>.Value;

    protected readonly App App;

    protected ActionImpl()
    {
        App = RuntimeContext.Current;
    }

    public void On(object target, in T e) => On((TSelf)target, in e);

    protected abstract void On(TSelf self, in T e);
}

public abstract class FuncImpl<TSelf, T, TResult> : ICallbackImpl<T, TResult> where T : ICallback<TResult>
{
    public static int Id => ICallback.Id<T>.Value;

    protected readonly App App;

    protected FuncImpl()
    {
        App = RuntimeContext.Current;
    }

    protected abstract TResult On(TSelf self, in T e);

    TResult ICallbackImpl<T, TResult>.On(object target, in T e)
    {
        return On((TSelf)target, in e);
    }

    void ICallbackImpl<T>.On(object target, in T e)
    {
        throw new NotImplementedException();
    }
}