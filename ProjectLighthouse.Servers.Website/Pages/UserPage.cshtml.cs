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
    public bool IsProfileUserHearted;

    public bool IsProfileUserBlocked;

    public UserEntity? ProfileUser;
    public UserPage(DatabaseContext database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int userId)
    {
        this.ProfileUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (this.ProfileUser == null) return this.NotFound();

        // Determine if user can view profile according to profileUser's privacy settings
        if (this.User == null || !this.User.IsAdmin)
        {
            switch (this.ProfileUser.ProfileVisibility)
            {
                case PrivacyType.PSN:
                {
                    if (this.User != null) return this.NotFound();

                    break;
                }
                case PrivacyType.Game:
                {
                    if (this.ProfileUser != this.User) return this.NotFound();

                    break;
                }
                case PrivacyType.All: break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        if (this.User == null) return this.Page();

        this.IsProfileUserHearted = await this.Database.HeartedProfiles
            .Where(h => h.HeartedUserId == this.ProfileUser.UserId)
            .Where(h => h.UserId == this.User.UserId)
            .AnyAsync();

        this.IsProfileUserBlocked = await this.Database.IsUserBlockedBy(this.ProfileUser.UserId, this.User.UserId);
        
        return this.Page();
    }
}