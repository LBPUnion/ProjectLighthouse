#nullable enable
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages;

public class PasswordResetRequiredPage : BaseLayout
{
    public PasswordResetRequiredPage([NotNull] Database database) : base(database)
    {}

    public bool WasResetRequest { get; private set; }

    public async Task<IActionResult> OnGet()
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");
        if (!user.PasswordResetRequired) return this.Redirect("~/passwordReset");

        return this.Page();
    }
}