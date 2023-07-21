using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: CreatePlaylist, HeartPlaylist, AddLevelToPlaylist
/// </summary>
public class PlaylistActivityEntity : ActivityEntity
{
    public int PlaylistId { get; set; }

    [ForeignKey(nameof(PlaylistId))]
    public PlaylistEntity Playlist { get; set; }
}