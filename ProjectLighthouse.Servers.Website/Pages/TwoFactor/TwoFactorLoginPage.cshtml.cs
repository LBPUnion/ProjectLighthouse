using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Localization.StringLists;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.TwoFactor;

public class TwoFactorLoginPage : BaseLayout
{
    public TwoFactorLoginPage(Database database) : base(database)
    { }

    public string Error { get; set; } = "";

    public IActionResult OnGet()
    {

        return this.Page();
    }

    public async Task<IActionResult> OnPost([FromForm] string? code)
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        if (!user.IsTwoFactorSetup) return this.RedirectToPage(nameof(UserSettingsPage));

        if (CryptoHelper.verifyCode(code, user.TwoFactorSecret))
        {
            user.TwoFactorBackup = null;
            user.TwoFactorSecret = null;

            await this.Database.SaveChangesAsync();

            return this.RedirectToPage(nameof(UserSettingsPage));
        }

        this.Error = this.Translate(TwoFactorStrings.InvalidCode);
        return this.Page();
    }
}