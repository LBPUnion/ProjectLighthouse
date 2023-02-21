#nullable enable
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Misc;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Users;
/// <summary>
/// Used by the games to update details about a user's profile
/// LBP1 only uses Location and IconHash
/// LBP2 and onward uses the rest
/// </summary>
public class UserUpdate
{
    [XmlElement("location")]
    public Location? Location { get; set; }

    [XmlElement("biography")]
    public string? Biography { get; set; }

    [XmlElement("icon")]
    public string? IconHash { get; set; }

    [XmlElement("planets")]
    public string? PlanetHash { get; set; }

    [XmlElement("crossControlPlanet")]
    public string? PlanetHashLBP2CC { get; set; }

    [XmlArray("slots")]
    [XmlArrayItem("slot")]
    public List<UserUpdateSlot>? Slots { get; set; }

    [XmlElement("yay2")]
    public string? YayHash { get; set; }

    [XmlElement("meh2")]
    public string? MehHash { get; set; }

    [XmlElement("boo2")]
    public string? BooHash { get; set; }
}

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