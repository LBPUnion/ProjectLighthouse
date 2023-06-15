using System.Collections.Generic;
using YamlDotNet.Serialization;

namespace LBPUnion.ProjectLighthouse.Configuration;

public enum FilterMode
{
    None,
    Asterisks,
    Random,
    Furry,
}

public class CensorConfiguration : ConfigurationBase<CensorConfiguration>
{
    // HEY, YOU!
    // THIS VALUE MUST BE INCREMENTED FOR EVERY CONFIG CHANGE!
    //
    // This is so Lighthouse can properly identify outdated configurations and update them with newer settings accordingly.
    // If you are modifying anything here, this value MUST be incremented.
    // Thanks for listening~
    public override int ConfigVersion { get; set; } = 1;
    public override string ConfigName { get; set; } = "censor.yml";
    public override bool NeedsConfiguration { get; set; } = false;

    public FilterMode UserInputFilterMode { get; set; } = FilterMode.None;

    // ReSharper disable once StringLiteralTypo
    public List<string> FilteredWordList { get; set; } = new()
    {
        "cunt",
        "fag",
        "faggot",
        "tranny",
        "dyke",
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
        "porn",
        "pornography",
    };
    
    public override ConfigurationBase<CensorConfiguration> Deserialize(IDeserializer deserializer, string text) => deserializer.Deserialize<CensorConfiguration>(text);
}