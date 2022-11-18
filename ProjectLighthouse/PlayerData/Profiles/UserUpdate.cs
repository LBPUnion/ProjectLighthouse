#nullable enable
using System.Collections.Generic;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.PlayerData.Profiles;

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