#nullable enable
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class LogoutPage : BaseLayout
{
    public LogoutPage(Database database) : base(database)
    {}
    public async Task<IActionResult> OnGet()
    {
        WebToken? token = this.Database.WebTokenFromRequest(this.Request);
        if (token == null) return this.BadRequest();

        this.Database.WebTokens.Remove(token);
        await this.Database.SaveChangesAsync();

        this.Response.Cookies.Delete("LighthouseToken");

        return this.Page();
    }
}