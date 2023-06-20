namespace XGFramework.Services;

/// <summary>
/// 使用TypeId的方式
/// </summary>
public interface IMessageFilter
{
    private static int index;

    protected static class TypeId<T>
    {
        public static readonly int Value;

        static TypeId()
        {
            Value = index++;
        }
    }

    static abstract int Id { get; }

    ValueTask<bool> On(MessageFilterContext context);
}

public abstract class MessageFilter<T> : IMessageFilter
{
    public static int Id => IMessageFilter.TypeId<T>.Value;

    protected readonly App App;

    protected MessageFilter()
    {
        App = RuntimeContext.Current;
    }

    public ValueTask<bool> On(MessageFilterContext context)
    {
        return On((T)context.Context.Target, context);
    }

    protected abstract ValueTask<bool> On(T self, MessageFilterContext context);
}