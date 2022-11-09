using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;

namespace LBPUnion.ProjectLighthouse.RecentActivity;

public class Activity
{
    [Key]
    public int EventId { get; set; }
    public EventType EventType { get; set; }
    public TargetType TargetType { get; set; }
    public int TargetId { get; set; }
    public long EventTimestamp { get; set; }

    public long Interaction1 { get; set; }
    public long Interaction2 { get; set; }

    [JsonIgnore]
    public virtual User Actor { get; set; }
}