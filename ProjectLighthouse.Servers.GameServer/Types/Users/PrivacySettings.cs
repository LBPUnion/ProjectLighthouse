#nullable enable
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Users;

[XmlRoot("privacySettings")]
[XmlType("privacySettings")]
public class PrivacySettings
{
    [XmlElement("levelVisiblity")]
    public string? LevelVisibility { get; set; }

    [XmlElement("profileVisiblity")]
    public string? ProfileVisibility { get; set; }
}