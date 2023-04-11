using System.Collections.Generic;
using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization;

[XmlRoot("playlists")]
public struct PlaylistResponse : ILbpSerializable
{
    [XmlElement("playlist")]
    public required List<GamePlaylist> Playlists { get; set; }

    [XmlAttribute("total")]
    public required int Total { get; set; }

    [XmlAttribute("hint_start")]
    public required int HintStart { get; set; }
}