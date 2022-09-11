#nullable enable
using System.ComponentModel.DataAnnotations;
using System.Text.RegularExpressions;
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

    private readonly Regex base64Regex = new(@"data:([^\/]+)\/([^;]+);base64,(.*)", RegexOptions.Compiled);

    private async Task<string?> parseAvatar(string? avatar)
    {
        if (string.IsNullOrWhiteSpace(avatar)) return null;

        System.Text.RegularExpressions.Match match = this.base64Regex.Match(avatar);

        if (!match.Success) return null;

        if (match.Groups.Count != 4) return null;

        byte[] data = Convert.FromBase64String(match.Groups[3].Value);

        LbpFile file = new(data);

        if (file.FileType is not (LbpFileType.Jpeg or LbpFileType.Png)) return null;

        string assetsDirectory = FileHelper.ResourcePath;
        string path = FileHelper.GetResourcePath(file.Hash);

        FileHelper.EnsureDirectoryCreated(assetsDirectory);
        await System.IO.File.WriteAllBytesAsync(path, file.Data);
        return file.Hash;
    }

    private static bool IsValidEmail(string? email) => !string.IsNullOrWhiteSpace(email) && new EmailAddressAttribute().IsValid(email);

    public async Task<IActionResult> OnPost([FromRoute] int userId, [FromForm] string? avatar, [FromForm] string? username, [FromForm] string? email, [FromForm] string? biography, [FromForm] string? timeZone, [FromForm] string? language)
    {
        this.ProfileUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (this.ProfileUser == null) return this.NotFound();

        if (this.User == null) return this.Redirect("~/user/" + userId);

        if (!this.User.IsModerator && this.User != this.ProfileUser) return this.Redirect("~/user/" + userId);

        string? avatarHash = await this.parseAvatar(avatar);

        if (avatarHash != null) this.ProfileUser.IconHash = avatarHash;

        if (biography != null && !this.ProfileUser.Biography.Equals(SanitizationHelper.SanitizeString(biography)))
            this.ProfileUser.Biography = SanitizationHelper.SanitizeString(biography);
        
        if (ServerConfiguration.Instance.Mail.MailEnabled && IsValidEmail(email) && this.User == this.ProfileUser || this.User.IsAdmin)
        {
            // if email hasn't already been used
            if (this.Database.Users.Any(u => u.EmailAddress != null && u.EmailAddress.ToLower().Equals(email!.ToLower())))
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
            if (language != null && this.ProfileUser.Language != language)
            {
                if (LocalizationManager.GetAvailableLanguages().Contains(language))
                    this.ProfileUser.Language = language;
            }

            if (timeZone != null && this.ProfileUser.TimeZone != timeZone)
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

        if(!this.User.IsModerator && this.User != this.ProfileUser) return this.Redirect("~/user/" + userId);

        return this.Page();
    }
}