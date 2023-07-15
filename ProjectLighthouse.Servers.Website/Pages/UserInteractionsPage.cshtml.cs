using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Moderation.Cases;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class UserInteractionsPage : BaseLayout
{
    public List<UserEntity> BlockedUsers = new();

    public bool CommentsDisabledByModerator;

    public UserEntity? ProfileUser;

    public UserInteractionsPage(DatabaseContext database) : base(database)
    { }

    public async Task<IActionResult> OnGet([FromRoute] int userId)
    {
        this.ProfileUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (this.ProfileUser == null) return this.NotFound();

        if (this.User == null) return this.Redirect("~/user/" + userId);
        if (!this.User.IsModerator && this.User != this.ProfileUser) return this.Redirect("~/user/" + userId);

        this.BlockedUsers = await this.Database.BlockedProfiles
            .Where(b => b.UserId == this.ProfileUser.UserId)
            .Select(b => b.BlockedUser)
            .ToListAsync();

        this.CommentsDisabledByModerator = await this.Database.Cases
            .Where(c => c.AffectedId == this.ProfileUser.UserId)
            .Where(c => c.Type == CaseType.UserDisableComments)
            .Where(c => c.Dismissed == false)
            .AnyAsync();

        return this.Page();
    }
}