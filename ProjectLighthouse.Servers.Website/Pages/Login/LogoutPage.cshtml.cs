#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Login;

public class LogoutPage : BaseLayout
{
    public LogoutPage(DatabaseContext database) : base(database)
    {}
    public async Task<IActionResult> OnGet()
    {
        WebTokenEntity? token = this.Database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/");

        this.Database.WebTokens.Remove(token);
        await this.Database.SaveChangesAsync();

        this.Response.Cookies.Delete("LighthouseToken");

        return this.Page();
    }
}