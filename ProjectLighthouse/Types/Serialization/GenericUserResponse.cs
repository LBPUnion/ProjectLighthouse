using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types.Filter;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

public struct GenericUserResponse<T> : ILbpSerializable, IHasCustomRoot where T : ILbpSerializable
{
    public GenericUserResponse() { }

    public GenericUserResponse(string rootElement, List<T> users, int total, int hintStart)
    {
        this.RootTag = rootElement;
        this.Users = users;
        this.Total = total;
        this.HintStart = hintStart;
    }

    public GenericUserResponse(string rootElement, List<T> users, PaginationData pageData)
    {
        this.RootTag = rootElement;
        this.Users = users;
        this.Total = pageData.TotalElements;
        this.HintStart = pageData.HintStart;
    }

    public GenericUserResponse(string rootElement, List<T> users)
    {
        this.RootTag = rootElement;
        this.Users = users;
    }

    [XmlIgnore]
    private string RootTag { get; }

    [XmlElement("user")]
    public List<T> Users { get; set; }

    [DefaultValue(0)]
    [XmlAttribute("total")]
    public int Total { get; set; }

    [DefaultValue(0)]
    [XmlAttribute("hint_start")]
    public int HintStart { get; set; }

    public string GetRoot() => this.RootTag;
}