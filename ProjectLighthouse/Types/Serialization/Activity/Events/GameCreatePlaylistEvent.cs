using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Database;

namespace LBPUnion.ProjectLighthouse.Types.Serialization.Activity.Events;

public class GameCreatePlaylistEvent : GameEvent
{
    [XmlElement("object_playlist_id")]
    public int TargetPlaylistId { get; set; }

    public new async Task PrepareSerialization(DatabaseContext database)
    {
        await base.PrepareSerialization(database);
    }
}