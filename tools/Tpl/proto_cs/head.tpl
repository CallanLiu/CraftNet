using ProtoBuf;
using CraftNet.Services;
{{ for item in using_list }}
using {{ item }};
{{- end }}

namespace {{namespace}}
{
    {{codes}}    
}