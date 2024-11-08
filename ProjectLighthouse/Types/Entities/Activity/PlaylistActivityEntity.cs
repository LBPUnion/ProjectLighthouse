using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Activity;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Activity;

/// <summary>
/// Supported event types: <see cref="EventType.CreatePlaylist"/> and <see cref="EventType.HeartPlaylist"/>.
/// </summary>
public class PlaylistActivityEntity : ActivityEntity
{
    /// <summary>
    /// The <see cref="PlaylistEntity.PlaylistId"/> of the <see cref="PlaylistEntity"/> that this event refers to.
    /// </summary>
    [Column("PlaylistId")]
    public int PlaylistId { get; set; }

    [ForeignKey(nameof(PlaylistId))]
    public PlaylistEntity Playlist { get; set; }
}

/// <summary>
/// Supported event types: <see cref="EventType.AddLevelToPlaylist"/>.
/// <remarks>
/// <para>
/// The relationship between <see cref="PlaylistActivityEntity"/> and <see cref="PlaylistWithSlotActivityEntity"/>
/// is slightly hacky but it allows us to reuse columns that would normally only be user with other <see cref="ActivityEntity"/> types.
/// </para> 
/// </remarks>
/// </summary>
public class PlaylistWithSlotActivityEntity : ActivityEntity
{
    /// <summary>
    /// The <see cref="PlaylistEntity.PlaylistId"/> of the <see cref="PlaylistEntity"/> that this event refers to.
    /// </summary> 
    [Column("PlaylistId")]
    public int PlaylistId { get; set; }

    [ForeignKey(nameof(PlaylistId))]
    public PlaylistEntity Playlist { get; set; }

    /// <summary>
    /// This reuses the SlotId column of <see cref="LevelActivityEntity"/> but has no ForeignKey definition so that it can be null
    /// <remarks>
    /// <para>
    /// It effectively serves as extra storage for PlaylistActivityEntity to use for the AddLevelToPlaylistEvent
    /// </para>
    /// </remarks>
    /// </summary>
    [Column("SlotId")]
    public int SlotId { get; set; }
}