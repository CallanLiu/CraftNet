namespace Demo.Entities;

public class GObject : IDisposable
{
    /// <summary>
    ///  版本
    ///     如果池化，每次放入池中版本将会+1
    /// </summary>
    public uint Version { get; protected set; } = 1;

    public virtual void Dispose()
    {
        // 已经销毁
        if (Version == 0)
            return;
        Version = 0;
        OnDestroy();
    }

    protected virtual void OnDestroy()
    {
    }
}