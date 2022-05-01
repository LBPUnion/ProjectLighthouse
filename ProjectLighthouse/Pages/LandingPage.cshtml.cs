#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages;

public class LandingPage : BaseLayout
{

    public int AuthenticationAttemptsCount;
    public List<User> PlayersOnline = new();

    public int PlayersOnlineCount;
    public LandingPage(Database database) : base(database)
    {}

    [UsedImplicitly]
    public async Task<IActionResult> OnGet()
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user != null && user.PasswordResetRequired) return this.Redirect("~/passwordResetRequired");

        this.PlayersOnlineCount = await StatisticsHelper.RecentMatches();

        if (user != null)
            this.AuthenticationAttemptsCount = await this.Database.AuthenticationAttempts.Include
                    (a => a.GameToken)
                .CountAsync(a => a.GameToken.UserId == user.UserId);

        List<int> userIds = await this.Database.LastContacts.Where(l => TimestampHelper.Timestamp - l.Timestamp < 300).Select(l => l.UserId).ToListAsync();

        this.PlayersOnline = await this.Database.Users.Where(u => userIds.Contains(u.UserId)).ToListAsync();
        return this.Page();
    }
}