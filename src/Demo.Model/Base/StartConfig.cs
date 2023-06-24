using System.Xml;
using System.Xml.Serialization;

namespace Demo;

public class StartConfig
{
    // 当前启动进程
    [XmlIgnore] public ushort PId { get; private set; }

    [XmlAttribute] public string Name { get; set; }
    [XmlElement("Process")] public ProcessConfig[] Processes { get; set; }


    [XmlIgnore] public ProcessConfig Current { get; private set; }


    public static StartConfig Load(ushort pid, string path)
    {
        XmlSerializer   serializer = new XmlSerializer(typeof(StartConfig), new XmlRootAttribute("Server"));
        using XmlReader reader     = new XmlTextReader(path);

        StartConfig config = (StartConfig)serializer.Deserialize(reader);
        config.PId = pid;

        foreach (var processConfig in config.Processes)
        {
            if (processConfig.Id == pid)
                config.Current = processConfig;
        }

        return config;
    }
}

public class ProcessConfig
{
    [XmlAttribute] public ushort Id { get; set; }
    [XmlAttribute] public string EndPoint { get; set; }
    [XmlAttribute] public string Urls { get; set; }
    [XmlElement("App")] public List<AppConfig> AppConfigs { get; set; }
}

public class AppConfig
{
    [XmlAttribute] public ushort Index { get; set; }

    [XmlAttribute] public string Name { get; set; }

    // [XmlAttribute] public string Bind { get; set; }
    [XmlAttribute] public string Type { get; set; }

    // public BindingAddress GetBindingAddress() => string.IsNullOrEmpty(Bind) ? null : BindingAddress.Parse(Bind);
}