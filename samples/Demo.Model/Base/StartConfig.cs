using System.Xml;
using System.Xml.Serialization;
using CraftNet;
using CraftNet.Services;

namespace Demo;

public class PropertyConfig
{
    [XmlAttribute] public string Name { get; set; }
    [XmlAttribute] public string Value { get; set; }
}

public class StartConfig
{
    // 当前启动进程
    [XmlIgnore] public ushort PId { get; private set; }

    [XmlAttribute] public string Name { get; set; }
    [XmlElement("Process")] public ProcessConfig[] Processes { get; set; }

    [XmlElement("Property")] public PropertyConfig[] PropertyConfigs { get; set; }

    [XmlIgnore] public ProcessConfig Current { get; private set; }

    [XmlIgnore] private readonly Dictionary<AppType, List<AppConfig>> _type2App   = new();
    [XmlIgnore] private readonly Dictionary<string, string>           _properties = new();

    public IReadOnlyList<AppConfig> GetAppConfigs(AppType type)
    {
        _type2App.TryGetValue(type, out var list);
        return list;
    }

    public static StartConfig Load(ushort pid, string path)
    {
        XmlSerializer   serializer = new XmlSerializer(typeof(StartConfig), new XmlRootAttribute("Server"));
        using XmlReader reader     = new XmlTextReader(path);

        StartConfig config = (StartConfig)serializer.Deserialize(reader);
        config!.PId = pid;

        foreach (var propertyConfig in config.PropertyConfigs)
        {
            config._properties.Add(propertyConfig.Name, propertyConfig.Value);
        }

        foreach (var processConfig in config.Processes)
        {
            if (processConfig.Id == pid)
                config.Current = processConfig;

            foreach (var appConfig in processConfig.AppConfigs)
            {
                if (!config._type2App.TryGetValue(appConfig.Type, out List<AppConfig> appConfigs))
                {
                    appConfigs = new List<AppConfig>();
                    config._type2App.Add(appConfig.Type, appConfigs);
                }

                appConfig.ProcessConfig = processConfig;
                appConfig.ActorId       = new ActorId(processConfig.Id, ActorType<App>.Value, appConfig.Index);
                appConfigs.Add(appConfig);
            }
        }

        return config;
    }

    public string GetProperty(string name)
    {
        _properties.TryGetValue(name, out var value);
        return value;
    }
}

public class ProcessConfig
{
    [XmlAttribute] public ushort Id { get; set; }
    [XmlAttribute] public string EndPoint { get; set; }
    [XmlAttribute] public string Urls { get; set; }
    [XmlElement("App")] public AppConfig[] AppConfigs { get; set; }
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