#nullable enable
using System.Text.RegularExpressions;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class SlotSettingsPage : BaseLayout
{

    public Slot? Slot;
    public SlotSettingsPage(Database database) : base(database)
    {}

    private readonly Regex base64Regex = new(@"data:([^\/]+)\/([^;]+);base64,(.*)", RegexOptions.Compiled);

    private async Task<string?> parseAvatar(string avatar)
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

    public async Task<IActionResult> OnPost([FromRoute] int slotId, [FromForm] string avatar, [FromForm] string name, [FromForm] string description, string labels)
    {
        this.Slot = await this.Database.Slots.FirstOrDefaultAsync(u => u.SlotId == slotId);
        if (this.Slot == null) return this.NotFound();

        if (this.User == null) return this.Redirect("~/slot/" + slotId);

        if (!this.User.IsModerator && this.User != this.Slot.Creator) return this.Redirect("~/slot/" + slotId);

        string? avatarHash = await this.parseAvatar(avatar);

        if (avatarHash != null) this.Slot.IconHash = avatarHash;

        if (!string.IsNullOrEmpty(name)) this.Slot.Name = SanitizationHelper.SanitizeString(name);
        
        if (!string.IsNullOrEmpty(description)) this.Slot.Description = SanitizationHelper.SanitizeString(description);

        if (!string.IsNullOrEmpty(labels)) this.Slot.AuthorLabels = LabelHelper.RemoveInvalidLabels(SanitizationHelper.SanitizeString(labels));

        // ReSharper disable once InvertIf
        if (this.Database.ChangeTracker.HasChanges())
        {
            this.Slot.LastUpdated = TimeHelper.TimestampMillis;
            await this.Database.SaveChangesAsync();
        }

        return this.Redirect("~/slot/" + slotId);
    }

    public async Task<IActionResult> OnGet([FromRoute] int slotId)
    {
        this.Slot = await this.Database.Slots.FirstOrDefaultAsync(s => s.SlotId == slotId);
        if (this.Slot == null) return this.NotFound();

        if (this.User == null) return this.Redirect("~/slot/" + slotId);

        if(!this.User.IsModerator && this.User.UserId != this.Slot.CreatorId) return this.Redirect("~/slot/" + slotId);

        return this.Page();
    }
}