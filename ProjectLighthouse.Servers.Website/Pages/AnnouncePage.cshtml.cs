#nullable enable

using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Website;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class AnnouncePage : BaseLayout
{
    public AnnouncePage(DatabaseContext database) : base(database)
    { }

    public List<WebsiteAnnouncementEntity> Announcements { get; set; } = new();
    public string Error { get; set; } = "";

    public async Task<IActionResult> OnGet()
    {
        this.Announcements = await this.Database.WebsiteAnnouncements
            .OrderByDescending(a => a.AnnouncementId)
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
            Title = title,
            Content = content,
        };

        await this.Database.WebsiteAnnouncements.AddAsync(announcement);
        await this.Database.SaveChangesAsync();

        if (DiscordConfiguration.Instance.DiscordIntegrationEnabled)
        {
            string truncatedAnnouncement = content.Length > 250 ? content[..250] + "..." : content;
            
            await WebhookHelper.SendWebhook(
                title: "A new announcement has been posted",
                description: $"{truncatedAnnouncement} ([read more]({ServerConfiguration.Instance.ExternalUrl}/announce))",
                dest: WebhookHelper.WebhookDestination.Public);    
        }

        return this.RedirectToPage();
    }

    public async Task<IActionResult> OnPostDelete(int id)
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.BadRequest();
        if (!user.IsAdmin) return this.BadRequest();

        WebsiteAnnouncementEntity? announcement = await this.Database.WebsiteAnnouncements
            .Where(a => a.AnnouncementId == id)
            .FirstOrDefaultAsync();

        if (announcement == null) return this.BadRequest();

        this.Database.WebsiteAnnouncements.Remove(announcement);
        await this.Database.SaveChangesAsync();

        return this.RedirectToPage();
    }
}