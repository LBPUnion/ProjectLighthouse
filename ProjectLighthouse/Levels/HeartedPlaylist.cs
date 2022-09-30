using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;

namespace LBPUnion.ProjectLighthouse.Levels;

public class HeartedPlaylist
{
    [Key]
    public int HeartedPlaylistId { get; set; }

    public int UserId { get; set; }

    [ForeignKey(nameof(UserId))]
    public User User { get; set; }

    public int PlaylistId { get; set; }

    [ForeignKey(nameof(PlaylistId))]
    public Playlist Playlist { get; set; }
}