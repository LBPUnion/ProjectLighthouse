using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Moderation;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Moderation.Cases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Login;

public class BannedUserPage : BaseLayout
{
    public BannedUserPage(DatabaseContext database) : base(database)
    { }

    public ModerationCaseEntity? ModCase;

    [UsedImplicitly]
    public async Task<IActionResult> OnGet()
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);

        if (user == null) return this.Redirect("~/login");
        if (!user.IsBanned) return this.Redirect("~/");

        ModerationCaseEntity? modCase = await this.Database.Cases.OrderByDescending(c => c.CreatedAt)
            .Where(c => c.AffectedId == user.UserId)
            .Where(c => c.Type == CaseType.UserBan)
            .Where(c => c.DismissedAt != null)
            .FirstOrDefaultAsync();

        if (modCase == null) Logger.Warn($"User {user.UserId} is banned but has no mod case?", LogArea.Login);

        this.ModCase = modCase;

        return this.Page();
    }
}