using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Profiles;

/// <summary>
///     The location of a slot on a planet.
/// </summary>
[XmlRoot("location")]
[XmlType("location")]
public class Location
{
    [XmlIgnore]
    public int Id { get; set; }

    [XmlElement("x")]
    public int X { get; set; }

    [XmlElement("y")]
    public int Y { get; set; }

    public string Serialize() => LbpSerializer.StringElement("x", this.X) + LbpSerializer.StringElement("y", this.Y);
}