using XGFramework.Services;
{{ for item in using_list }}
using {{ item }};
{{- end }}

namespace {{namespace}};

{{- for e in elems }}
{{if string.ends_with e.name "Req"}}
public partial class {{e.name}} : IRequest, IMessageMeta
{{else if string.ends_with e.name "Resp"}}
public partial class {{e.name}} : IResponse, IMessageMeta
{{else if string.ends_with e.name "Msg"}}
public partial class {{e.name}} : IMessage, IMessageMeta
{{end -}}
{
    public static ushort Opcode => {{for.index + opcode}};
    ushort IMessageBase.GetOpcode() => Opcode;
{{- for child in e.elems }}
    {{- if child.is_repeated }}
    public List<{{child.field_type}}> {{child.name}} { get; set; }
    {{- else }}
    public {{child.field_type}} {{child.name}} { get; set; }
    {{- end }}
{{- end }}
}
{{- end }}