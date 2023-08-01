using System.Reflection;
using Serilog;

namespace CraftNet.Services;

public sealed class MessageDescCollection : IMessageDescCollection
{
    private readonly Dictionary<ushort, MessageDesc> _types       = new();
    private readonly Dictionary<Type, MessageDesc>   _typeToIdMap = new();
    private readonly IMessageSerializerProvider      _messageSerializerProvider;

    public MessageDescCollection(IMessageSerializerProvider messageSerializerProvider)
    {
        _messageSerializerProvider = messageSerializerProvider;
    }

    public void Add<T>() where T : IMessageBase, IMessageMeta
    {
        Add(typeof(T), T.Opcode);
    }

    public void Add(Assembly assembly)
    {
        Type[] types = assembly.GetTypes();
        foreach (var type in types)
        {
            if (type.IsInterface || type.IsAbstract)
                continue;

            if (!typeof(IMessageBase).IsAssignableFrom(type))
                continue;

            PropertyInfo propertyInfo = type.GetProperty("Opcode");
            if (propertyInfo is null)
                continue;
            object value = propertyInfo.GetValue(null);
            if (value is null)
                continue;

            this.Add(type, (ushort)value);
            Log.Debug("MessageDescCollection: {Op}={Type}", value, type.Name);
        }
    }

    /// <summary>
    /// 添加类型
    /// </summary>
    /// <param name="type"></param>
    /// <param name="opcode"></param>
    public void Add(Type type, ushort opcode)
    {
        IMessageSerializer messageSerializer = type.GetCustomAttribute<MessageSerializerAttribute>() ??
                                               _messageSerializerProvider.Get(type, opcode);
        MessageDesc messageDesc = new MessageDesc(opcode, type, messageSerializer);
        _types.Add(opcode, messageDesc);
        _typeToIdMap.Add(type, messageDesc);
    }

    /// <summary> 
    /// 添加类型并指定序列化器
    /// </summary>
    /// <param name="type"></param>
    /// <param name="opcode"></param>
    /// <param name="messageSerializer"></param>
    public void Add(Type type, ushort opcode, IMessageSerializer messageSerializer)
    {
        MessageDesc messageDesc = new MessageDesc(opcode, type, messageSerializer);
        _types[opcode]     = messageDesc;
        _typeToIdMap[type] = messageDesc;
    }

    /// <summary>
    /// 获取opcode
    /// </summary>
    /// <param name="type"></param>
    /// <param name="desc"></param>
    /// <returns></returns>
    public bool TryGet(Type type, out MessageDesc desc) => _typeToIdMap.TryGetValue(type, out desc);

    /// <summary>
    /// 获取类型
    /// </summary>
    /// <param name="opcode"></param>
    /// <param name="desc"></param>
    /// <returns></returns>
    public bool TryGet(ushort opcode, out MessageDesc desc) => _types.TryGetValue(opcode, out desc);
}