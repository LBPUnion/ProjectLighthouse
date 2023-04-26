using System.Collections.Generic;
using System.ComponentModel;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

public struct GenericPlaylistResponse<T> : ILbpSerializable, IHasCustomRoot where T : ILbpSerializable
{
    public GenericPlaylistResponse()
    { }

    public GenericPlaylistResponse(string rootElement, List<T> playlists, int total, int hintStart)
    {
        this.RootTag = rootElement;
        this.Playlists = playlists;
        this.Total = total;
        this.HintStart = hintStart;
    }

    public GenericPlaylistResponse(string rootElement, List<T> playlists)
    {
        this.RootTag = rootElement;
        this.Playlists = playlists;
    }

    [XmlIgnore]
    public string RootTag { get; set; }

    [XmlElement("playlist")]
    public List<T> Playlists { get; set; }

    [DefaultValue(0)]
    [XmlAttribute("total")]
    public int Total { get; set; }

    [DefaultValue(0)]
    [XmlAttribute("hint_start")]
    public int HintStart { get; set; }

    public string GetRoot() => this.RootTag;
}