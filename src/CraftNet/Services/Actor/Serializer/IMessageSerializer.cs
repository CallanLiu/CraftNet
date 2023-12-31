﻿using System.Buffers;
using System.IO.Pipelines;

namespace CraftNet.Services;

/// <summary>
/// 消息序列化
/// </summary>
public interface IMessageSerializer
{
    object Deserialize(Type type, ReadOnlySequence<byte> buffer);

    void Serialize(object obj, PipeWriter pipeWriter);
}

[AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
public abstract class MessageSerializerAttribute : Attribute, IMessageSerializer
{
    public abstract object Deserialize(Type type, ReadOnlySequence<byte> buffer);

    public abstract void Serialize(object obj, PipeWriter pipeWriter);
}