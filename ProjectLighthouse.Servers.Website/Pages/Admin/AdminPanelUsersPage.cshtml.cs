#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Admin;

public class AdminPanelUsersPage : BaseLayout
{
    public int UserCount;

    public List<UserEntity> Users = new();

    public int PageAmount;
    public int PageNumber;
    public string SearchValue = "";

    public AdminPanelUsersPage(DatabaseContext database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int pageNumber, [FromQuery] string? name)
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");
        if (!user.IsAdmin) return this.NotFound();

        if (string.IsNullOrWhiteSpace(name)) name = "";

        this.SearchValue = name.Replace(" ", string.Empty);

        this.UserCount = await this.Database.Users.CountAsync(u => u.Username.Contains(this.SearchValue));
        
        this.PageNumber = pageNumber;
        this.PageAmount = Math.Max(1, (int)Math.Ceiling((double)this.UserCount / ServerStatics.PageSize));

        if (this.PageNumber < 0 || this.PageNumber >= this.PageAmount)
            return this.Redirect($"/admin/users/{Math.Clamp(this.PageNumber, 0, this.PageAmount - 1)}");

        this.Users = await this.Database.Users
            .OrderByDescending(u => u.PermissionLevel)
            .ThenByDescending(u => u.UserId)
            .Where(u => u.Username.Contains(this.SearchValue))
            .Skip(pageNumber * ServerStatics.PageSize)
            .Take(ServerStatics.PageSize)
            .ToListAsync();

        return this.Page();
    }
}