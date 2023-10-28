using System;
using System.Text;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Types.Entities.Notifications;
using LBPUnion.ProjectLighthouse.Types.Notifications;

namespace LBPUnion.ProjectLighthouse.Database;

public partial class DatabaseContext
{
    /// <summary>
    ///     Sends a notification to a user.
    /// </summary>
    /// <param name="userId">The user ID of the target user.</param>
    /// <param name="text">The message to send.</param>
    /// <param name="type">The <see cref="NotificationType"/> for the notification. Defaults to <c>ModerationNotification</c>.</param>
    public async Task SendNotification
        (int userId, string text, NotificationType type = NotificationType.ModerationNotification)
    {
        StringBuilder builder = new(text);

        // Append server name to notification text if enabled
        if (ServerConfiguration.Instance.NotificationConfiguration.ShowServerNameInText)
        {
            builder.Insert(0, $"[{ServerConfiguration.Instance.Customization.ServerName}] ");
        }
        // Append timestamp to notification text if enabled
        if (ServerConfiguration.Instance.NotificationConfiguration.ShowTimestampInText)
        {
            builder.Insert(0, $"[{DateTime.Now:HH:mm:ss}] ");
        }

        NotificationEntity notification = new()
        {
            UserId = userId,
            Type = type,
            Text = builder.ToString(),
        };

        await this.Notifications.AddAsync(notification);
        await this.SaveChangesAsync();
    }
}