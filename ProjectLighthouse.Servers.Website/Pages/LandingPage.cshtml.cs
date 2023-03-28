#nullable enable
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class LandingPage : BaseLayout
{
    public LandingPage(DatabaseContext database) : base(database)
    {}

    public int PendingAuthAttempts;
    public List<UserEntity> PlayersOnline = new();

    public List<SlotEntity>? LatestTeamPicks;
    public List<SlotEntity>? NewestLevels;

    [UsedImplicitly]
    public async Task<IActionResult> OnGet()
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user != null && user.PasswordResetRequired) return this.Redirect("~/passwordResetRequired");

        if (user != null)
            this.PendingAuthAttempts = await this.Database.PlatformLinkAttempts
                .CountAsync(l => l.UserId == user.UserId);

        List<int> userIds = await this.Database.LastContacts.Where(l => TimeHelper.Timestamp - l.Timestamp < 300).Select(l => l.UserId).ToListAsync();

        this.PlayersOnline = await this.Database.Users.Where(u => userIds.Contains(u.UserId)).ToListAsync();

        const int maxShownLevels = 5;

        this.LatestTeamPicks = await this.Database.Slots.Where(s => s.Type == SlotType.User && !s.SubLevel)
            .Where(s => s.TeamPick)
            .OrderByDescending(s => s.FirstUploaded)
            .Take(maxShownLevels)
            .Include(s => s.Creator)
            .ToListAsync();

        this.NewestLevels = await this.Database.Slots.Where(s => s.Type == SlotType.User && !s.SubLevel)
            .OrderByDescending(s => s.FirstUploaded)
            .Take(maxShownLevels)
            .Include(s => s.Creator)
            .ToListAsync();

        return this.Page();
    }
}