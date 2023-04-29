using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

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