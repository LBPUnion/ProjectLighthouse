using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Notifications;
using LBPUnion.ProjectLighthouse.Types.Serialization;

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

    public static GameNotification ConvertToGame(NotificationEntity notification) => new()
    {
        Type = notification.Type,
        Text = notification.Text,
    };
}