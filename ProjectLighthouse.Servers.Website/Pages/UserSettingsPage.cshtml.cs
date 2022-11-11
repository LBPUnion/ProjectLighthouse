#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Localization;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class UserSettingsPage : BaseLayout
{

    public User? ProfileUser;
    public UserSettingsPage(Database database) : base(database)
    {}

    private static bool IsValidEmail(string? email) => !string.IsNullOrWhiteSpace(email) && new EmailAddressAttribute().IsValid(email);

    [SuppressMessage("ReSharper", "SpecifyStringComparison")]
    public async Task<IActionResult> OnPost([FromRoute] int userId, [FromForm] string? avatar, [FromForm] string? username, [FromForm] string? email, [FromForm] string? biography, [FromForm] string? timeZone, [FromForm] string? language)
    {
        this.ProfileUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (this.ProfileUser == null) return this.NotFound();

        if (this.User == null) return this.Redirect("~/user/" + userId);

        if (!this.User.IsModerator && this.User != this.ProfileUser) return this.Redirect("~/user/" + userId);

        string? avatarHash = await FileHelper.ParseBase64Image(avatar);

        if (avatarHash != null) this.ProfileUser.IconHash = avatarHash;

        biography = SanitizationHelper.SanitizeString(biography);

        if (this.ProfileUser.Biography != biography && biography.Length <= 512) this.ProfileUser.Biography = biography;

        if (ServerConfiguration.Instance.Mail.MailEnabled && IsValidEmail(email) && (this.User == this.ProfileUser || this.User.IsAdmin))
        {
            // if email hasn't already been used
            if (!await this.Database.Users.AnyAsync(u => u.EmailAddress != null && u.EmailAddress.ToLower() == email!.ToLower()))
            {
                if (this.ProfileUser.EmailAddress != email)
                {
                    this.ProfileUser.EmailAddress = email;
                    this.ProfileUser.EmailAddressVerified = false;
                }
            }
        }

        if (this.ProfileUser == this.User)
        {
            if (!string.IsNullOrWhiteSpace(language) && this.ProfileUser.Language != language)
            {
                if (LocalizationManager.GetAvailableLanguages().Contains(language))
                    this.ProfileUser.Language = language;
            }

            if (!string.IsNullOrWhiteSpace(timeZone) && this.ProfileUser.TimeZone != timeZone)
            {
                HashSet<string> timeZoneIds = TimeZoneInfo.GetSystemTimeZones().Select(t => t.Id).ToHashSet();
                if (timeZoneIds.Contains(timeZone)) this.ProfileUser.TimeZone = timeZone;
            }
        }


        await this.Database.SaveChangesAsync();
        return this.Redirect("~/user/" + userId);
    }

    public async Task<IActionResult> OnGet([FromRoute] int userId)
    {
        this.ProfileUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (this.ProfileUser == null) return this.NotFound();

        if (this.User == null) return this.Redirect("~/user/" + userId);

        if (!this.User.IsModerator && this.User != this.ProfileUser) return this.Redirect("~/user/" + userId);

        return this.Page();
    }
}