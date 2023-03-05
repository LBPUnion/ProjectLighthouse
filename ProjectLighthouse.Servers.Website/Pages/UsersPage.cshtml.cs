#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class UsersPage : BaseLayout
{
    public int PageAmount;

    public int PageNumber;

    public int UserCount;

    public List<UserEntity> Users = new();

    public string? SearchValue;

    public UsersPage(DatabaseContext database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int pageNumber, [FromQuery] string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) name = "";

        this.SearchValue = name.Replace(" ", string.Empty);

        this.UserCount = await this.Database.Users.CountAsync(u => u.PermissionLevel != PermissionLevel.Banned && u.Username.Contains(this.SearchValue));

        this.PageNumber = pageNumber;
        this.PageAmount = Math.Max(1, (int)Math.Ceiling((double)this.UserCount / ServerStatics.PageSize));

        if (this.PageNumber < 0 || this.PageNumber >= this.PageAmount) return this.Redirect($"/users/{Math.Clamp(this.PageNumber, 0, this.PageAmount - 1)}");

        this.Users = await this.Database.Users.Where(u => u.PermissionLevel != PermissionLevel.Banned && u.Username.Contains(this.SearchValue))
            .Where(u => u.ProfileVisibility == PrivacyType.All) // TODO: change check for when user is logged in
            .OrderByDescending(b => b.UserId)
            .Skip(pageNumber * ServerStatics.PageSize)
            .Take(ServerStatics.PageSize)
            .ToListAsync();

        return this.Page();
    }
}