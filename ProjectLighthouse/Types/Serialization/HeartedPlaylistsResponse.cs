using System.Collections.Generic;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

//TODO figure out how to abstract responses and dynamically modify XmlRoot 
[XmlRoot("favouritePlaylists")]
public class HeartedPlaylistResponse : ILbpSerializable
{
    [XmlElement("playlist")]
    public required List<Playlist> Playlists { get; set; }

    [XmlAttribute("total")]
    public required int Total { get; set; }

    [XmlAttribute("hint_start")]
    public required int HintStart { get; set; }
}