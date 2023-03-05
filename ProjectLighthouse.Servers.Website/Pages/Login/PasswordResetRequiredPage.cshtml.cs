#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Login;

public class PasswordResetRequiredPage : BaseLayout
{
    public PasswordResetRequiredPage(DatabaseContext database) : base(database)
    {}

    public bool WasResetRequest { get; private set; }

    public IActionResult OnGet()
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");
        if (!user.PasswordResetRequired) return this.Redirect("~/passwordReset");

        return this.Page();
    }
}