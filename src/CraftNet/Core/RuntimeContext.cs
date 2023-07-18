namespace CraftNet;

public static class RuntimeContext
{
    [ThreadStatic] private static App _current;

    public static App Current => _current;

    internal static void Set(App ctx) => _current = ctx;

    internal static void Reset() => _current = null;
}