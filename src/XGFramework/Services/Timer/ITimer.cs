namespace XGFramework.Services;

public interface ITimer : IDisposable
{
    int Type { get; }
    object State { get; }
}