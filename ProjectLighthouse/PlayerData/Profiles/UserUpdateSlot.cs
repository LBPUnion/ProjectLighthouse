#nullable enable
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Levels;

namespace LBPUnion.ProjectLighthouse.PlayerData.Profiles;

[XmlRoot("slot")]
public class UserUpdateSlot
{

    [XmlElement("type")]
    public SlotType? Type { get; set; }

    [XmlElement("id")]
    public int? SlotId { get; set; }

    [XmlElement("location")]
    public Location? Location { get; set; }

}