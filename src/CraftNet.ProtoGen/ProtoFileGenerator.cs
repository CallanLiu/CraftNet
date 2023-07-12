// using System.Collections.Immutable;
// using System.Diagnostics;
// using System.Reflection;
// using System.Text;
// using Microsoft.CodeAnalysis;
// using Microsoft.CodeAnalysis.Text;
// using Scriban;
//
// namespace CraftNet.ProtoGen;
//
// [Generator(LanguageNames.CSharp)]
// public class ProtoFileGenerator : ISourceGenerator
// {
//     public void Execute(GeneratorExecutionContext context)
//     {
//         var schemas = context.AdditionalFiles.Where(at => at.Path.EndsWith(".proto")).ToImmutableArray();
//
//         if (schemas.IsDefaultOrEmpty)
//             return;
//
//         Debugger.Launch();
//
//         Stream stream = Assembly.GetExecutingAssembly().GetManifestResourceStream("CraftNet.ProtoGen.Cs.tpl");
//         using StreamReader sr = new StreamReader(stream);
//         string str = sr.ReadToEnd();
//
//         Template template = Template.Parse(str);
//         foreach (AdditionalText item in schemas)
//         {
//             SourceText   sourceText          = item.GetText();
//             string       txt                 = sourceText.ToString();
//             ProtoGenData protoElementDefines = Proto2CS.Gen(txt);
//             string       code                = template.Render(protoElementDefines);
//
//             // StringBuilder stringBuilder       = protoElementDefines.Gen();
//             //File.WriteAllText(Path.Combine(Path.GetDirectoryName(item.Path), $"{Path.GetFileName(item.Path)}.cs"), stringBuilder.ToString());
//             context.AddSource(Path.ChangeExtension(Path.GetFileName(item.Path), ".g.cs"),
//                 SourceText.From(code.ToString(), Encoding.UTF8));
//         }
//     }
//
//     public void Initialize(GeneratorInitializationContext context)
//     {
//     }
// }