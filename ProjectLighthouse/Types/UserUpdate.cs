#nullable enable
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Profiles;

namespace LBPUnion.ProjectLighthouse.Types;

public class UserUpdate
{
    [XmlElement("location")]
    public Location? Location;

    [XmlElement("biography")]
    public string? Biography;

    [XmlElement("icon")]
    public string? IconHash;

    [XmlElement("planets")]
    public string? PlanetHash;

    [XmlElement("yay2")]
    public string? YayHash;

    [XmlElement("meh2")]
    public string? MehHash;

    [XmlElement("boo2")]
    public string? BooHash;
}