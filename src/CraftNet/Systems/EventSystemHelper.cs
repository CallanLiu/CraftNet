namespace CraftNet;

public static class EventSystemHelper
{
    public static async ValueTask TriggerAsync<T>(IApp app, T e) where T : IEvent
    {
        await app.Scheduler.Yield();
        App a = (App)app;
        a.GetSystem<IEventSystem>().Trigger(e);
    }

    public static async ValueTask<TResult> InvokeAsync<T, TResult>(IApp app, T e) where T : IEvent
    {
        await app.Scheduler.Yield();
        App a = (App)app;
        return a.GetSystem<IEventSystem>().Invoke<T, TResult>(e);
    }
}