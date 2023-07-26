namespace Demo.Entities;

public abstract class ComponentBase : GObject
{
    public sealed override void Dispose()
    {
        base.Dispose();
    }
}