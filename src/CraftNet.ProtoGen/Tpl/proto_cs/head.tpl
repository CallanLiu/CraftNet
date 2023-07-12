using ProtoBuf;
using XGFramework.Services;
{{ for item in using_list }}
using {{ item }};
{{- end }}

namespace {{namespace}};