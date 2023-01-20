using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Types;
using YamlDotNet.Serialization;

namespace LBPUnion.ProjectLighthouse.Configuration;

public class CensorConfiguration : ConfigurationBase<CensorConfiguration>
{
    public override int ConfigVersion { get; set; } = 1;
    public override string ConfigName { get; set; } = "censor.yml";
    public override bool NeedsConfiguration { get; set; } = false;

    public FilterMode UserInputFilterMode { get; set; } = FilterMode.None;

    public List<string> FilteredWordList { get; set; } = new()
    {
        "cunt",
        "fag",
        "faggot",
        "horny",
        "kook",
        "kys",
        "loli",
        "nigga",
        "nigger",
        "penis",
        "pussy",
        "retard",
        "retarded",
        "vagina",
        "vore",
        "restitched",
        "h4h",
    };
    
    public override ConfigurationBase<CensorConfiguration> Deserialize(IDeserializer deserializer, string text) => deserializer.Deserialize<CensorConfiguration>(text);
}