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
            ActivityGroupType.User => this.TargetUserId ?? -1,
            ActivityGroupType.Level => this.TargetSlotId ?? -1,
            ActivityGroupType.Playlist => this.TargetPlaylistId ?? -1,
            ActivityGroupType.News => this.TargetNewsId ?? -1,
            _ => this.Activity.UserId,
        };

    public ActivityGroupType GroupType =>
        this.TargetPlaylistId != null
            ? ActivityGroupType.Playlist
            : this.TargetNewsId != null
                ? ActivityGroupType.News
                : this.TargetSlotId != null
                    ? ActivityGroupType.Level
                    : ActivityGroupType.User;
}