using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Users;

/// <summary>
/// Sent by the game client to inform the server
/// of the user's friend list
/// Used to filter activities from friends
/// </summary>
[XmlRoot("npdata")]
[XmlType("npdata")]
public class NPData
{
    [XmlArray("friends")]
    [XmlArrayItem("npHandle")]
    public List<string>? Friends { get; set; }

    [XmlArray("blocked")]
    [XmlArrayItem("npHandle")]
    public List<string>? BlockedUsers { get; set; }
}