#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class SlotPage : BaseLayout
{

    public SlotEntity? Slot;
    public SlotPage(DatabaseContext database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int id)
    {
        SlotEntity? slot = await this.Database.Slots.Include(s => s.Creator)
            .Where(s => s.Type == SlotType.User || (this.User != null && this.User.PermissionLevel >= PermissionLevel.Moderator))
            .FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();
        System.Diagnostics.Debug.Assert(slot.Creator != null);

        // Determine if user can view slot according to creator's privacy settings
        if (this.User == null || !this.User.IsAdmin)
        {
            switch (slot.Creator.ProfileVisibility)
            {
                case PrivacyType.PSN:
                {
                    if (this.User != null) return this.NotFound();

                    break;
                }
                case PrivacyType.Game:
                {
                    if (this.User == null || slot.Creator != this.User) return this.NotFound();

                    break;
                }
                case PrivacyType.All: break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        if ((slot.Hidden || slot.SubLevel && this.User == null && this.User != slot.Creator) && !(this.User?.IsModerator ?? false))
            return this.NotFound();

        this.Slot = slot;

        return this.Page();
    }
}
