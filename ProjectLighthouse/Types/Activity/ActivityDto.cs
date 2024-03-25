using LBPUnion.ProjectLighthouse.Types.Entities.Activity;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Types.Activity;

public class ActivityDto
{
    public required ActivityEntity Activity { get; set; }
    public int? TargetSlotId { get; set; }
    public int? TargetSlotCreatorId { get; set; }
    public GameVersion? TargetSlotGameVersion { get; set; }
    public int? TargetUserId { get; set; }
    public int? TargetPlaylistId { get; set; }
    public int? TargetNewsId { get; set; }
    public int? TargetTeamPickId { get; set; }

    public int TargetId =>
        this.GroupType switch
        {
            ActivityGroupType.User => this.TargetUserId ?? 0,
            ActivityGroupType.Level => this.TargetSlotId ?? 0,
            ActivityGroupType.Playlist => this.TargetPlaylistId ?? 0,
            ActivityGroupType.News => this.TargetNewsId ?? 0,
            _ => this.Activity.UserId,
        };

    public ActivityGroupType GroupType =>
        this.TargetSlotId != null
            ? ActivityGroupType.Level
            : this.TargetUserId != null
                ? ActivityGroupType.User
                : this.TargetPlaylistId != null
                    ? ActivityGroupType.Playlist
                    : ActivityGroupType.News;
}