using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Notifications;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class NotificationsPage : BaseLayout
{
    public NotificationsPage(DatabaseContext database) : base(database)
    { }

    public List<WebsiteAnnouncementEntity> Announcements { get; set; } = new();
    public List<NotificationEntity> Notifications { get; set; } = new();
    public string Error { get; set; } = "";

    public async Task<IActionResult> OnGet()
    {
        this.Announcements = await this.Database.WebsiteAnnouncements
            .Include(a => a.Publisher)
            .OrderByDescending(a => a.AnnouncementId)
            .ToListAsync();

        if (this.User == null) return this.Page();

        this.Notifications = await this.Database.Notifications
            .Where(n => n.UserId == this.User.UserId)
            .Where(n => !n.IsDismissed)
            .OrderByDescending(n => n.Id)
            .ToListAsync();

        return this.Page();
    }

    public async Task<IActionResult> OnPost([FromForm] string title, [FromForm] string content)
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.BadRequest();
        if (!user.IsAdmin) return this.BadRequest();

        if (string.IsNullOrWhiteSpace(title) || string.IsNullOrWhiteSpace(content))
        {
            this.Error = "Invalid form data, please ensure all fields are filled out.";
            return this.Page();
        }

        WebsiteAnnouncementEntity announcement = new()
        {
            Title = title.Trim(),
            Content = content.Trim(),
            PublisherId = user.UserId,
        };

        this.Database.WebsiteAnnouncements.Add(announcement);
        await this.Database.SaveChangesAsync();

        if (!DiscordConfiguration.Instance.DiscordIntegrationEnabled) return this.RedirectToPage();

        string truncatedAnnouncement = content.Length > 250
            ? content[..250] + $"... [read more]({ServerConfiguration.Instance.ExternalUrl}/notifications)"
            : content;

        await WebhookHelper.SendWebhook($":mega: {title}", truncatedAnnouncement);

        return this.RedirectToPage();
    }

    public async Task<IActionResult> OnPostDelete(string type, int id)
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.BadRequest();

        switch (type)
        {
            case "announcement":
            {
                WebsiteAnnouncementEntity? announcement = await this.Database.WebsiteAnnouncements
                    .Where(a => a.AnnouncementId == id)
                    .FirstOrDefaultAsync();

                if (announcement == null || !user.IsAdmin) return this.BadRequest();

                this.Database.WebsiteAnnouncements.Remove(announcement);
                await this.Database.SaveChangesAsync();

                break;
            }
            case "notification":
            {
                NotificationEntity? notification = await this.Database.Notifications
                    .Where(n => n.Id == id)
                    .Where(n => !n.IsDismissed)
                    .FirstOrDefaultAsync();

                if (notification == null || notification.UserId != user.UserId) return this.BadRequest();

                notification.IsDismissed = true;

                await this.Database.SaveChangesAsync();

                break;
            }
        }

        return this.RedirectToPage();
    }
}