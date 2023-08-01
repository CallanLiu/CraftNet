namespace CraftNet.Services;

public static class ResponseCompletionSourcePool
{
    public static ResponseCompletionSource<T> Get<T>(bool autoReturn = true)
    {
        ResponseCompletionSource<T> tcs = TypePool<T>.Pool.Get();
        return tcs;
    }

    public static void Return<T>(ResponseCompletionSource<T> obj) => TypePool<T>.Pool.Return(obj);

    private static class TypePool<T>
    {
        public static readonly ConcurrentObjectPool<ResponseCompletionSource<T>> Pool =
            new ConcurrentObjectPool<ResponseCompletionSource<T>>();
    }
}