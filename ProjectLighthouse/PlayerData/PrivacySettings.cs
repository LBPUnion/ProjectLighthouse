#nullable enable
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.PlayerData;

[XmlRoot("privacySettings")]
[XmlType("privacySettings")]
public class PrivacySettings
{
    [XmlElement("levelVisiblity")]
    public string? LevelVisibility { get; set; }

    [XmlElement("profileVisiblity")]
    public string? ProfileVisibility { get; set; }

    public string Serialize()
        => LbpSerializer.StringElement
        (
            "privacySettings",
            LbpSerializer.StringElement("levelVisibility", this.LevelVisibility) +
            LbpSerializer.StringElement("profileVisibility", this.ProfileVisibility)
        );
}