using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Types.Entities.Notifications;
using LBPUnion.ProjectLighthouse.Types.Notifications;

namespace LBPUnion.ProjectLighthouse.Database;

public partial class DatabaseContext
{
    public async Task SendNotification
        (int userId, string text, NotificationType type = NotificationType.ModerationNotification)
    {
        // Append server name to notification text if enabled
        if (ServerConfiguration.Instance.NotificationConfiguration.ShowServerNameInText)
        {
            text = $"[{ServerConfiguration.Instance.Customization.ServerName}] {text}";
        }

        NotificationEntity notification = new()
        {
            UserId = userId,
            Type = type,
            Text = text,
        };

        await this.Notifications.AddAsync(notification);
        await this.SaveChangesAsync();
    }
}