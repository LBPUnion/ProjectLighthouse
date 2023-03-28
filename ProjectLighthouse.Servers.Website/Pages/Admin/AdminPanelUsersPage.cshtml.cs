#nullable enable
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
    public AdminPanelUsersPage(DatabaseContext database) : base(database)
    {}

    public async Task<IActionResult> OnGet()
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");
        if (!user.IsAdmin) return this.NotFound();

        this.Users = await this.Database.Users
            .OrderByDescending(u => u.PermissionLevel)
            .ThenByDescending(u => u.UserId)
            .ToListAsync();
        
        this.UserCount = this.Users.Count;

        return this.Page();
    }
}