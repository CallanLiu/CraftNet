namespace XGFramework;

public class RpcException : Exception
{
}

public class RpcTimeoutException : RpcException
{
    public static RpcTimeoutException Default { get; } = new();
}

/// <summary>
/// Id溢出导致与等待响应字典里面的Key冲突的异常(等待响应字典里面堆积了大量请求)
/// </summary>
public class RpcIdOverflowException : RpcException
{
    public static RpcIdOverflowException Default { get; } = new();
}