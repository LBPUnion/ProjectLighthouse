#nullable enable
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Login;

public class PasswordResetRequiredPage : BaseLayout
{
    public PasswordResetRequiredPage(Database database) : base(database)
    {}

    public bool WasResetRequest { get; private set; }

    public IActionResult OnGet()
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");
        if (!user.PasswordResetRequired) return this.Redirect("~/passwordReset");

        return this.Page();
    }
}