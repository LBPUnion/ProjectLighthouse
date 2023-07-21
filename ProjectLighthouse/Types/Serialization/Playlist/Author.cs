using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Playlist;

[XmlRoot("author")]
public struct Author : ILbpSerializable
{
    public Author() { }

    public Author(string username)
    {
        this.Username = username;
    }

    [XmlElement("npHandle")]
    public string Username { get; set; }
}