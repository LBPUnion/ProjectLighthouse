using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Moderation.Cases;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class UserPrivacyPage : BaseLayout
{
    public List<UserEntity> BlockedUsers = new();

    public bool CommentsDisabledByModerator;

    public UserEntity? ProfileUser;

    public UserPrivacyPage(DatabaseContext database) : base(database)
    { }

    public async Task<IActionResult> OnGet([FromRoute] int userId)
    {
        this.ProfileUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (this.ProfileUser == null) return this.NotFound();

        if (this.User == null) return this.Redirect("~/user/" + userId);
        if (this.User != this.ProfileUser) return this.Redirect("~/user/" + userId);

        this.BlockedUsers = await this.Database.BlockedProfiles.Where(b => b.UserId == this.ProfileUser.UserId)
            .Select(b => b.BlockedUser)
            .ToListAsync();

        this.CommentsDisabledByModerator = await this.Database.Cases.Where(c => c.AffectedId == this.ProfileUser.UserId)
            .Where(c => c.Type == CaseType.UserDisableComments)
            .Where(c => c.DismissedAt == null)
            .AnyAsync();

        return this.Page();
    }

    public async Task<IActionResult> OnPost([FromRoute] int userId, [FromForm] string profilePrivacyLevel, [FromForm] string profileCommentsEnabled, [FromForm] string slotPrivacyLevel)
    {
        this.ProfileUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (this.ProfileUser == null) return this.NotFound();

        if (this.User == null) return this.Redirect("~/user/" + userId);
        if (this.User != this.ProfileUser) return this.Redirect("~/user/" + userId);

        this.CommentsDisabledByModerator = await this.Database.Cases.Where(c => c.AffectedId == this.ProfileUser.UserId)
            .Where(c => c.Type == CaseType.UserDisableComments)
            .Where(c => c.DismissedAt == null)
            .AnyAsync();

        if (!this.CommentsDisabledByModerator)
        {
            this.ProfileUser.CommentsEnabled = profileCommentsEnabled switch
            {
                "true" => true,
                "false" => false,
                _ => this.ProfileUser.CommentsEnabled,
            };
        }
        else
        {
            this.ProfileUser.CommentsEnabled = false;
        }

        this.ProfileUser.ProfileVisibility = profilePrivacyLevel switch
        {
            "public" => PrivacyType.All,
            "signedInOnly" => PrivacyType.PSN,
            "inGameOnly" => PrivacyType.Game,
            _ => this.ProfileUser.ProfileVisibility,
        };

        this.ProfileUser.LevelVisibility = slotPrivacyLevel switch
        {
            "public" => PrivacyType.All,
            "signedInOnly" => PrivacyType.PSN,
            "inGameOnly" => PrivacyType.Game,
            _ => this.ProfileUser.LevelVisibility,
        };

        await this.Database.SaveChangesAsync();

        return this.Redirect($"~/user/{userId}");
    }
}