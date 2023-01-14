#nullable enable
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

    public async Task<IActionResult> OnPost([FromRoute] int slotId, [FromForm] string? avatar, [FromForm] string? name, [FromForm] string? description, string? labels)
    {
        this.Slot = await this.Database.Slots.FirstOrDefaultAsync(u => u.SlotId == slotId);
        if (this.Slot == null) return this.NotFound();

        if (this.User == null) return this.Redirect("~/slot/" + slotId);

        if (!this.User.IsModerator && this.User != this.Slot.Creator) return this.Redirect("~/slot/" + slotId);

        string? avatarHash = await FileHelper.ParseBase64Image(avatar);

        if (avatarHash != null) this.Slot.IconHash = avatarHash;

        name = SanitizationHelper.SanitizeString(name);
        if (this.Slot.Name != name && name.Length <= 64) this.Slot.Name = name;

        description = SanitizationHelper.SanitizeString(description);
        if (this.Slot.Description != description && description.Length <= 512) this.Slot.Description = description;

        labels = LabelHelper.RemoveInvalidLabels(SanitizationHelper.SanitizeString(labels));
        if (this.Slot.AuthorLabels != labels) this.Slot.AuthorLabels = labels;

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

        if (!this.User.IsModerator && this.User.UserId != this.Slot.CreatorId) return this.Redirect("~/slot/" + slotId);

        return this.Page();
    }
}