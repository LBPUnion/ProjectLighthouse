using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: CreatePlaylist, HeartPlaylist
/// </summary>
public class PlaylistActivityEntity : ActivityEntity
{
    [Column("PlaylistId")]
    public int PlaylistId { get; set; }

    [ForeignKey(nameof(PlaylistId))]
    public PlaylistEntity Playlist { get; set; }
}

/// <summary>
/// Supported event types: AddLevelToPlaylist
/// <para>
/// The relationship between <see cref="PlaylistActivityEntity"/> and <see cref="PlaylistWithSlotActivityEntity"/>
/// is slightly hacky but it allows conditional reuse of columns from other ActivityEntity's 
/// 
/// </para>
/// </summary>
public class PlaylistWithSlotActivityEntity : ActivityEntity
{
    [Column("PlaylistId")]
    public int PlaylistId { get; set; }

    [ForeignKey(nameof(PlaylistId))]
    public PlaylistEntity Playlist { get; set; }

    /// <summary>
    /// This reuses the SlotId column of <see cref="LevelActivityEntity"/> but has no ForeignKey definition so that it can be null
    /// <para>It effectively serves as extra storage for PlaylistActivityEntity to use for the AddLevelToPlaylistEvent</para>
    /// </summary>
    [Column("SlotId")]
    public int SlotId { get; set; }
}