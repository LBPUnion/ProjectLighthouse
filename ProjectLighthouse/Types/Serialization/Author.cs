using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("author")]
public class Author : ILbpSerializable
{
    public Author() { }

    public Author(string username)
    {
        this.Username = username;
    }

    [XmlElement("npHandle")]
    public string Username { get; set; }
}