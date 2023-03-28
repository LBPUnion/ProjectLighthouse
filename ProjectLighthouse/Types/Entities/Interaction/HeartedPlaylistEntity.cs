using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Interaction;

public class HeartedPlaylistEntity
{
    [Key]
    public int HeartedPlaylistId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public UserEntity User { get; set; }

    public int PlaylistId { get; set; }

    [ForeignKey(nameof(PlaylistId))]
    public PlaylistEntity Playlist { get; set; }
}