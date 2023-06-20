namespace XGFramework;

public static class RuntimeContext
{
    [ThreadStatic] private static App current;

    public static App Current => current;

    internal static void Set(App ctx) => current = ctx;

    internal static void Reset() => current = null;
}