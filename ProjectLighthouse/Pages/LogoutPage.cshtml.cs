#nullable enable
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages;

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