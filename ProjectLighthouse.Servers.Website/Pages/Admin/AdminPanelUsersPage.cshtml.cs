#nullable enable
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Admin;

public class AdminPanelUsersPage : BaseLayout
{
    public int UserCount;

    public List<User> Users = new();
    public AdminPanelUsersPage(Database database) : base(database)
    {}

    public async Task<IActionResult> OnGet()
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");
        if (!user.IsAdmin) return this.NotFound();

        this.Users = await this.Database.Users.OrderByDescending(u => u.UserId).ToListAsync();
        this.UserCount = this.Users.Count;

        return this.Page();
    }
}