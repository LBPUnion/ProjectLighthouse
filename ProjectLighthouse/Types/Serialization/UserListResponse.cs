using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

public struct UserListResponse : ILbpSerializable, IHasCustomRoot
{
    public UserListResponse() { }

    public UserListResponse(string rootElement, List<GameUser> users, int total, int hintStart)
    {
        this.RootTag = rootElement;
        this.Users = users;
        this.Total = total;
        this.HintStart = hintStart;
    }

    public UserListResponse(string rootElement, List<GameUser> users)
    {
        this.RootTag = rootElement;
        this.Users = users;
    }

    [XmlIgnore]
    public string RootTag { get; set; }

    [XmlElement("user")]
    public List<GameUser> Users { get; set; }

    [DefaultValue(0)]
    [XmlAttribute("total")]
    public int Total { get; set; }

    [DefaultValue(0)]
    [XmlAttribute("hint_start")]
    public int HintStart { get; set; }

    public string GetRoot() => this.RootTag;
}