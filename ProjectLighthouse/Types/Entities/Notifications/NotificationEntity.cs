using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Notifications;

namespace LBPUnion.ProjectLighthouse.Types.Entities.Notifications;

public class NotificationEntity
{
    [Key]
    public int Id { get; set; }

    public int UserId { get; set; }

    #nullable enable

    [ForeignKey(nameof (UserId))]
    public UserEntity? User { get; set; }

    #nullable disable

    public NotificationType Type { get; set; } = NotificationType.ModerationNotification;

    public string Text { get; set; } = "";

    public bool IsDismissed { get; set; }
}