{{
    bool is_msg = true
    if string.ends_with parent_class 'Msg'
        _t = ': IMessage, IMessageMeta'
    else if string.ends_with parent_class 'Req'
        _t = ': IRequest, IMessageMeta'
    else if string.ends_with parent_class 'Resp'
        _t = ': IResponse, IMessageMeta'
    else 
        is_msg = false
    end
}}
[MemoryPackable]
public partial class {{ name }} {{_t}}
{
    {{~ if is_msg ~}}
    public static ushort Opcode => {{ opcode }};
    ushort IMessageBase.GetOpcode() => Opcode;
    {{~ end ~}}
{{~ for e in elems ~}}
    {{- if e.type == "ProtoMessageField" ~}}
        {{~ if !(string.empty e.tail_comment)~}}
    /// <summary>
    /// {{ e.tail_comment }}
    /// </summary>
        {{~ end ~}}
        {{~ if e.is_repeated ~}}
    [MemoryPackOrder({{e.index}})] public List<{{e.field_type}}> {{e.name}} { get; set; } 
        {{~ else ~}}
    [MemoryPackOrder({{e.index}})] public {{e.field_type}} {{e.name}} { get; set; }
        {{~ end ~}}
    {{~ else if e.type == "ProtoComment" ~}}
    {{ e.comment }}
    {{~ end ~}}
{{~ end ~}}
    {{ enum_codes ~}}
    {{ msg_codes }}
}