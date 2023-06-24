using CommandLine;
using Scriban;

namespace XGFramework.ProtoGen;

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

    public static void Main(string[] args)
    {
        Options options = null;
        Parser.Default.ParseArguments<Options>(args)
            .WithNotParsed(error => throw new Exception($"命令行格式错误!"))
            .WithParsed(o => { options = o; });

        string tpl      = File.ReadAllText("./Tpl/" + options.Tpl);
        var    template = Template.Parse(tpl);

        if (template.HasErrors)
        {
            foreach (var msg in template.Messages)
            {
                Console.WriteLine(msg);
            }

            Console.ReadKey();
            return;
        }


        string outExt  = Path.GetFileNameWithoutExtension(options.Tpl);
        string fileExt = outExt.Split('_')[1];

        if (File.Exists(options.Input))
        {
            options.OutputPath = Path.Combine(Path.GetDirectoryName(options.Input), "Gen");
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
            string protoTxt = File.ReadAllText(file);
            var    data     = Proto2CS.Gen(protoTxt);
            var    code     = template.Render(data);

            string fileName       = Path.GetFileNameWithoutExtension(file);
            string outputFileName = Path.Combine(options.OutputPath, $"{fileName}.{fileExt}");
            File.WriteAllText(outputFileName, code);
            Console.WriteLine($"输出: {outputFileName}");
        }
    }
}