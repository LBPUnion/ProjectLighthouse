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

    public async Task<IActionResult> OnGet([FromRoute] int pageNumber, [FromQuery] string? name)
    {
        WebTokenEntity? token = this.Database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("/login");

        this.Users = await this.Database.Users
            .Where(u => u.PermissionLevel < 0)
            .Skip(pageNumber * ServerStatics.PageSize)
            .Take(ServerStatics.PageSize)
            .ToListAsync();

        this.UserCount = await this.Database.Users.CountAsync(u => u.PermissionLevel < 0);

        this.PageAmount = Math.Max(1, (int)Math.Ceiling((double)this.UserCount / ServerStatics.PageSize));
        
        return this.Page();
    }
}