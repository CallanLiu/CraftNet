namespace XGFramework;

public interface ISystemTypeId
{
}

public interface ISystemBase
{
    static virtual int GetId() => 0;
}

public interface ISystemBase<T> : ISystemBase where T : ISystemTypeId
{
    static int ISystemBase.GetId() => ISystemCollection.Index<T>.Value;
}