#nullable enable
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class LandingPage : BaseLayout
{
    public LandingPage(Database database) : base(database)
    {}

    public int PendingAuthAttempts;
    public List<User> PlayersOnline = new();

    public int PlayersOnlineCount;

    public List<Slot>? LatestTeamPicks;
    public List<Slot>? NewestLevels;

    [UsedImplicitly]
    public async Task<IActionResult> OnGet()
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user != null && user.PasswordResetRequired) return this.Redirect("~/passwordResetRequired");

        this.PlayersOnlineCount = await StatisticsHelper.RecentMatches();

        if (user != null)
            this.PendingAuthAttempts = await this.Database.AuthenticationAttempts.Include
                    (a => a.GameToken)
                .CountAsync(a => a.GameToken.UserId == user.UserId);

        List<int> userIds = await this.Database.LastContacts.Where(l => TimeHelper.Timestamp - l.Timestamp < 300).Select(l => l.UserId).ToListAsync();

        this.PlayersOnline = await this.Database.Users.Where(u => userIds.Contains(u.UserId)).ToListAsync();

        const int maxShownLevels = 5;

        this.LatestTeamPicks = await this.Database.Slots.Where
                (s => s.TeamPick)
            .OrderByDescending(s => s.FirstUploaded)
            .Take(maxShownLevels)
            .Include(s => s.Creator)
            .ToListAsync();

        this.NewestLevels = await this.Database.Slots.OrderByDescending(s => s.FirstUploaded).Take(maxShownLevels).Include(s => s.Creator).ToListAsync();

        return this.Page();
    }

    public ViewDataDictionary GetSlotViewData(int slotId, bool isMobile = false)
        => new(ViewData)
        {
            {
                "User", this.User
            },
            {
                "CallbackUrl", $"~/slot/{slotId}"
            },
            {
                "ShowLink", true
            },
            {
                "IsMini", true
            },
            {
                "IsMobile", isMobile
            },
        };
}