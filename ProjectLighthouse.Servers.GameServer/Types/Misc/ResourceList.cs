using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Serialization;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Misc;

/// <summary>
/// Used by the game to send a list of hashed resources
/// Use cases include requesting which resources are filtered
/// or what resources aren't currently uploaded to the server
/// </summary>
[XmlRoot("resources")]
[XmlType("resources")]
public class ResourceList : ILbpSerializable
{
    [XmlElement("resource")]
    public string[]? Resources;
}