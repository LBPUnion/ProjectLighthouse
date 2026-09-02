#nullable enable
using System.ComponentModel;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Levels;

public class CategoryDefaults
{
    [DefaultValue("")]
    [XmlElement("gameFilter")]
    public string? GameFilter { get; set; }

    [DefaultValue("")]
    [XmlElement("dateFilterType")]
    public string? DateFilterType { get; set; }

    [DefaultValue(null)]
    [XmlElement("includePlayed")]
    public bool? IncludePlayed { get; set; }

    [DefaultValue("")]
    [XmlElement("teamPicked")]
    public string? TeamPicked { get; set; }

    [DefaultValue("")]
    [XmlElement("blacklisted")]
    public string? Blacklisted { get; set; }
}
