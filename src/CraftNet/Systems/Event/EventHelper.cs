namespace CraftNet;

public static class EventHelper
{
    /// <summary>
    /// 跨线程调用
    /// </summary>
    /// <param name="self"></param>
    /// <param name="args"></param>
    /// <typeparam name="T"></typeparam>
    public static async ValueTask InvokeAsync<T>(IApp self, T args) where T : ICallback<ValueTask>
    {
        await self.Scheduler.Yield();
        App app = (App)self;
        await app.GetSystem<IEventSystem>().Invoke<T, ValueTask>(args);
    }

    /// <summary>
    /// 跨线程调用
    /// </summary>
    /// <param name="self"></param>
    /// <param name="args"></param>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TResult"></typeparam>
    /// <returns></returns>
    public static async ValueTask<TResult> InvokeAsync<T, TResult>(IApp self, T args)
        where T : ICallback<TResult>
    {
        await self.Scheduler.Yield();
        App app = (App)self;
        return app.GetSystem<IEventSystem>().Invoke<T, TResult>(args);
    }
}