using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Misc;

/// <summary>
///     The location of a slot on a planet.
/// </summary>
[XmlRoot("location")]
[XmlType("location")]
public class Location
{
    [XmlElement("x")]
    public int X { get; set; }

    [XmlElement("y")]
    public int Y { get; set; }

    public string Serialize() => LbpSerializer.StringElement("x", this.X) + LbpSerializer.StringElement("y", this.Y);
}