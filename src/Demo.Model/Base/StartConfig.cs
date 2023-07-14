using System.Xml;
using System.Xml.Serialization;
using CraftNet.Services;

namespace Demo;

public class StartConfig
{
    // 当前启动进程
    [XmlIgnore] public ushort PId { get; private set; }

    [XmlAttribute] public string Name { get; set; }
    [XmlElement("Process")] public ProcessConfig[] Processes { get; set; }


    [XmlIgnore] public ProcessConfig Current { get; private set; }

    [XmlIgnore] private Dictionary<AppType, List<AppConfig>> type2App = new();

    public IReadOnlyList<AppConfig> GetAppConfigs(AppType type)
    {
        type2App.TryGetValue(type, out var list);
        return list;
    }

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

            foreach (var appConfig in processConfig.AppConfigs)
            {
                if (!config.type2App.TryGetValue(appConfig.Type, out List<AppConfig> appConfigs))
                {
                    appConfigs = new List<AppConfig>();
                    config.type2App.Add(appConfig.Type, appConfigs);
                }

                appConfig.ProcessConfig = processConfig;
                appConfig.ActorId       = new ActorId(processConfig.Id, appConfig.Index);
                appConfigs.Add(appConfig);
            }
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
    [XmlIgnore] public ActorId ActorId { get; internal set; }

    [XmlAttribute] public ushort Index { get; set; }

    [XmlAttribute] public string Name { get; set; }

    [XmlAttribute] public AppType Type { get; set; }

    /// <summary>
    /// 所在进程配置
    /// </summary>
    [XmlIgnore]
    public ProcessConfig ProcessConfig { get; internal set; }
}