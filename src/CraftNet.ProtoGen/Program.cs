using System.Text;
using CommandLine;
using Scriban;

namespace CraftNet.ProtoGen;

public class Program
{
    public class Options
    {
        // 输入目录
        [Option("in", Required = true)] public string Input { get; set; }

        // 输出目录
        [Option("out", Required = false)] public string OutputPath { get; set; }

        [Option("tpl", Required = false, Default = "json_cs.tpl")]
        public string Tpl { get; set; }
    }

    public static async Task Main(string[] args)
    {
        Options options = null;
        Parser.Default.ParseArguments<Options>(args)
            .WithNotParsed(error => throw new Exception($"命令行格式错误!"))
            .WithParsed(o => { options = o; });

        Template headTpl = LoadTpl("head");
        Template enumTpl = LoadTpl("enum");
        Template msgTpl  = LoadTpl("message");

        Template LoadTpl(string key)
        {
            string tpl      = File.ReadAllText(Path.Combine("./Tpl/", options.Tpl, key + ".tpl"));
            var    template = Template.Parse(tpl);

            if (template.HasErrors)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                foreach (var msg in template.Messages)
                {
                    Console.WriteLine($"{key} {msg}");
                }

                Console.ReadKey();
                return null;
            }

            return template;
        }

        string fileExt = options.Tpl.Split('_')[^1];
        if (File.Exists(options.Input))
        {
            if (string.IsNullOrEmpty(options.OutputPath))
            {
                options.OutputPath = Path.Combine(Path.GetDirectoryName(options.Input), "Gen");
            }

            if (!Directory.Exists(options.OutputPath))
            {
                Directory.CreateDirectory(options.OutputPath);
            }

            Gen(options.Input);
        }
        else
        {
            if (string.IsNullOrEmpty(options.OutputPath))
            {
                options.OutputPath = Path.Combine(options.Input, "Gen");
                if (!Directory.Exists(options.OutputPath))
                {
                    Directory.CreateDirectory(options.OutputPath);
                }
            }

            string[] files = Directory.GetFiles(options.Input, "*.proto");
            foreach (var file in files)
            {
                Gen(file);
            }
        }

        void Gen(string file)
        {
            StringBuilder stringBuilder = new StringBuilder();

            string protoTxt = File.ReadAllText(file);
            var    data     = Proto2CS.Gen(protoTxt);


            ushort opcode = 0;
            foreach (var e in data.Elems)
            {
                Template curTpl = null;
                if (e is ProtoEnum)
                {
                    curTpl = enumTpl;
                }
                else if (e is ProtoMessage protoMessage)
                {
                    protoMessage.Opcode = data.Opcode + opcode++;
                    curTpl              = msgTpl;

                    // 嵌套枚举
                    StringBuilder temp = new StringBuilder();
                    foreach (var inner in protoMessage.Elems)
                    {
                        if (inner is ProtoEnum)
                        {
                            temp.AppendLine(enumTpl.Render(inner));
                        }
                    }

                    protoMessage.EnumCodes = temp.ToString();
                }

                if (curTpl != null)
                {
                    try
                    {
                        stringBuilder.AppendLine(curTpl.Render(e));
                    }
                    catch (Exception exception)
                    {
                        Console.ForegroundColor = ConsoleColor.Red;
                        Console.WriteLine($"{e.GetType().Name} {exception}");
                        return;
                    }
                }
            }

            data.Codes = stringBuilder.ToString();
            string finalCodes = headTpl.Render(data);

            string fileName       = Path.GetFileNameWithoutExtension(file);
            string outputFileName = Path.Combine(options.OutputPath, $"{fileName}.{fileExt}");
            File.WriteAllText(outputFileName, finalCodes);
            Console.WriteLine($"输出: {outputFileName}");
        }
    }
}