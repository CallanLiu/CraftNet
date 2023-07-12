namespace CraftNet;

public static class SetupExtensions
{
    public static ISetup AddSystem<T, TImpl>(this ISetup self, uint group = 0) where T : ISystemBase
        where TImpl : T
    {
        self.App.AddSystem<T, TImpl>(group);
        return self;
    }
}