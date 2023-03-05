using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

public class NpHandle : ILbpSerializable
{
    public NpHandle() { }

    public NpHandle(string username, string iconHash)
    {
        this.Username = username;
        this.IconHash = iconHash;
    }

    [XmlText]
    public string Username { get; set; }

    [XmlAttribute("icon")]
    public string IconHash { get; set; }
}