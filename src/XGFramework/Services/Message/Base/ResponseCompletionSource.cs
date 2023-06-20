using System.Threading.Tasks.Sources;

namespace XGFramework.Services;

public interface IResponseCompletionSource<TResponse>
{
    ushort Opcode { get; }

    void Complete(TResponse response);

    void Complete(ushort opcode, TResponse response);

    void SetException(Exception error);
}

/// <summary>
/// 完成时，会回到之前的Task上下文中进行执行。
/// </summary>
/// <typeparam name="TResponse"></typeparam>
public sealed class ResponseCompletionSource<TResponse> : IResponseCompletionSource<TResponse>,
    IValueTaskSource<TResponse>
{
    ManualResetValueTaskSourceCore<TResponse> core;

    public ushort Opcode { get; private set; }

    public ValueTask<TResponse> AsValueTask() => new(this, core.Version);

    public void Complete(TResponse response) => SetResult(response);

    public void Complete(ushort opcode, TResponse response)
    {
        this.Opcode = opcode;
        this.SetResult(response);
    }

    public void SetException(Exception error)
    {
        core.SetException(error);
    }

    private void SetResult(TResponse result)
    {
        core.SetResult(result);
    }

    public TResponse GetResult(short token)
    {
        bool isValid = token == core.Version;
        try
        {
            return core.GetResult(token);
        }
        finally
        {
            if (isValid)
            {
                Reset();
            }
        }
    }

    public ValueTaskSourceStatus GetStatus(short token) => core.GetStatus(token);

    public void OnCompleted(Action<object> continuation, object state, short token,
        ValueTaskSourceOnCompletedFlags flags) => core.OnCompleted(continuation, state, token, flags);

    private void Reset()
    {
        core.Reset();
        ResponseCompletionSourcePool.Return(this);
    }
}