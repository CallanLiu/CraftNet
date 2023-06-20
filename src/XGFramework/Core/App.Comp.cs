namespace XGFramework;

public partial class App : IComponentCollection
{
    private readonly IComponentCollection _components = new ComponentCollection();

    public T GetComp<T>() where T : IComp => _components.GetComp<T>();

    public T AddComp<T>() where T : IComp, new() => _components.AddComp<T>();

    public bool RemoveComp<T>() where T : IComp => _components.RemoveComp<T>();
}