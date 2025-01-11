using System;
using System.Text;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Types.Entities.Notifications;
using LBPUnion.ProjectLighthouse.Types.Notifications;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Database;

public partial class DatabaseContext
{
    /// <summary>
    ///     Sends a notification to a user.
    /// </summary>
    /// <param name="userId">The user ID of the target user.</param>
    /// <param name="text">The message to send.</param>
    /// <param name="prefix">Prepend server name/timestamp.</param>
    /// <param name="type">The <see cref="NotificationType"/> for the notification. Defaults to <c>ModerationNotification</c>.</param>
    public async Task SendNotification
        (int userId, string text, bool prefix = true, NotificationType type = NotificationType.ModerationNotification)
    {
        if (!await this.Users.AnyAsync(u => u.UserId == userId))
        {
            return;
        }

        if (string.IsNullOrWhiteSpace(text) || text.Length > 2048)
        {
            return;
        }

        StringBuilder builder = new(text);

        if (prefix)
        {
            // Prepend server name to notification text if enabled
            if (ServerConfiguration.Instance.NotificationConfiguration.ShowServerNameInText)
            {
                builder.Insert(0, $"[{ServerConfiguration.Instance.Customization.ServerName}] ");
            }

            // Prepend timestamp to notification text if enabled
            if (ServerConfiguration.Instance.NotificationConfiguration.ShowTimestampInText)
            {
                builder.Insert(0, $"[{DateTime.Now:g}] ");
            }
        }

        NotificationEntity notification = new()
        {
            UserId = userId,
            Type = type,
            Text = builder.ToString(),
        };

        this.Notifications.Add(notification);
        await this.SaveChangesAsync();
    }
}