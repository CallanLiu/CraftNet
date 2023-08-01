namespace Demo.Entities;

public abstract class ComponentBase : GObject
{
    public Entity Entity { get; internal set; }

    protected override void OnDestroy()
    {
    }
}