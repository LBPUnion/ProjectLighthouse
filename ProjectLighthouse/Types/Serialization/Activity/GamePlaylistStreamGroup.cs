using System.Xml.Serialization;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity;

public class GamePlaylistStreamGroup : GameStreamGroup
{
    [XmlElement("playlist_id")]
    public int PlaylistId { get; set; }
}