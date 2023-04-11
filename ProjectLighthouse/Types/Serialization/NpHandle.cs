using System.ComponentModel;
using System.Xml.Serialization;

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

    [DefaultValue("")]
    [XmlAttribute("icon")]
    public string IconHash { get; set; }
}