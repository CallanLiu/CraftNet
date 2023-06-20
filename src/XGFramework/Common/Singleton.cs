namespace XGFramework;

public class Singleton<TSelf> where TSelf : Singleton<TSelf>, new()
{
    private static TSelf _ins = new();
    public static TSelf Ins => _ins;
}