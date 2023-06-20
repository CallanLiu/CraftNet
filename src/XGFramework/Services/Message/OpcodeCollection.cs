namespace XGFramework.Services;

public sealed class OpcodeCollection : Singleton<OpcodeCollection>
{
    private readonly Dictionary<ushort, Type> _types       = new Dictionary<ushort, Type>();
    private readonly Dictionary<Type, ushort> _typeToIdMap = new Dictionary<Type, ushort>();

    public void Add<T>() where T : IMessageBase, IMessageMeta
    {
        Add(typeof(T), T.Opcode);
    }

    /// <summary>
    /// 添加类型
    /// </summary>
    /// <param name="type"></param>
    /// <param name="opcode"></param>
    public void Add(Type type, ushort opcode)
    {
        _types.Add(opcode, type);
        _typeToIdMap.Add(type, opcode);
    }

    /// <summary>
    /// 获取opcode
    /// </summary>
    /// <param name="type"></param>
    /// <param name="opcode"></param>
    /// <returns></returns>
    public bool TryGetOpcode(Type type, out ushort opcode) => _typeToIdMap.TryGetValue(type, out opcode);

    /// <summary>
    /// 获取类型
    /// </summary>
    /// <param name="opcode"></param>
    /// <param name="type"></param>
    /// <returns></returns>
    public bool TryGetType(ushort opcode, out Type type) => _types.TryGetValue(opcode, out type);
}