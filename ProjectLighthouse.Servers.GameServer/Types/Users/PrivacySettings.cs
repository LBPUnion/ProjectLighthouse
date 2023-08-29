using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Serialization;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Users;

[XmlRoot("privacySettings")]
[XmlType("privacySettings")]
public class PrivacySettings : ILbpSerializable
{
    [XmlElement("levelVisibility")]
    public string? LevelVisibility { get; set; }

    [XmlElement("profileVisibility")]
    public string? ProfileVisibility { get; set; }
}