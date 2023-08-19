#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class UserPage : BaseLayout
{
    public bool CanViewProfile;

    public bool IsProfileUserHearted;
    public bool IsProfileUserBlocked;

    public UserEntity? ProfileUser;
    public UserPage(DatabaseContext database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int userId)
    {
        this.ProfileUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (this.ProfileUser == null) return this.NotFound();

        this.CanViewProfile = this.ProfileUser.ProfileVisibility.CanAccess(this.User != null,
            this.ProfileUser == this.User || this.User != null && this.User.IsModerator);

        if (this.User == null) return this.Page();

        this.IsProfileUserHearted = await this.Database.HeartedProfiles
            .Where(h => h.HeartedUserId == this.ProfileUser.UserId)
            .Where(h => h.UserId == this.User.UserId)
            .AnyAsync();

        this.IsProfileUserBlocked = await this.Database.IsUserBlockedBy(this.ProfileUser.UserId, this.User.UserId);
        
        return this.Page();
    }
}