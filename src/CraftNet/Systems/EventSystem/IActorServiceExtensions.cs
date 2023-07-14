using CraftNet.Services;

namespace CraftNet;

public static class IActorServiceExtensions
{
    public static async ValueTask InvokeAsync<T>(this IActorService self, ActorId id, T args)
        where T : ICallback<ValueTask>
    {
        ActorMailbox queue = self.GetMailbox(id);
        if (queue is null)
            throw new Exception($"Actor不存在: id={id.ToString()} T={typeof(T).Name}");

        // 切换到目标上下文中
        await queue.App.Scheduler.Yield();

        App app = (App)queue.App;

        // 调用回调
        await app.GetSystem<IEventSystem>().Invoke<T, ValueTask>(args, queue.Target);
    }

    public static async ValueTask<TResult> InvokeAsync<T, TResult>(this IActorService self, ActorId id, T args)
        where T : ICallback<ValueTask<TResult>>
    {
        ActorMailbox queue = self.GetMailbox(id);
        if (queue is null)
            throw new Exception($"Actor不存在: id={id.ToString()} T={typeof(T).Name}");

        // 切换到目标上下文中
        await queue.App.Scheduler.Yield();

        App app = (App)queue.App;

        // 调用回调
        return await app.GetSystem<IEventSystem>().Invoke<T, ValueTask<TResult>>(args, queue.Target);
    }
}