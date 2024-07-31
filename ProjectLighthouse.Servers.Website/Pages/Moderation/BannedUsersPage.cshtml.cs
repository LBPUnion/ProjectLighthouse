using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Moderation;

public class BannedUsersPage : BaseLayout
{
    public BannedUsersPage(DatabaseContext database) : base(database)
    {}

    public List<UserEntity> Users = new();

    public int PageAmount;

    public int PageNumber;

    public int UserCount;

    public async Task<IActionResult> OnGet([FromRoute] int pageNumber)
    {
        WebTokenEntity? token = this.Database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("/login");

        this.UserCount = await this.Database.Users.CountAsync(u => u.PermissionLevel < 0);

        this.PageNumber = pageNumber;
        this.PageAmount = Math.Max(1, (int)Math.Ceiling((double)this.UserCount / ServerStatics.PageSize));

        if (this.PageNumber < 0 || this.PageNumber >= this.PageAmount)
            return this.Redirect($"/moderation/bannedUsers/{Math.Clamp(this.PageNumber, 0, this.PageAmount - 1)}");

        this.Users = await this.Database.Users.Where(u => u.PermissionLevel < 0)
            .OrderByDescending(u => u.UserId)
            .Skip(pageNumber * ServerStatics.PageSize)
            .Take(ServerStatics.PageSize)
            .ToListAsync();

        return this.Page();
    }
}