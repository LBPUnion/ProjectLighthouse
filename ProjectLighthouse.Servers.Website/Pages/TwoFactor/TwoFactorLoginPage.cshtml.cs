using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Localization.StringLists;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.TwoFactor;

public class TwoFactorLoginPage : BaseLayout
{
    public TwoFactorLoginPage(Database database) : base(database)
    { }

    public string Error { get; set; } = "";
    public string RedirectUrl { get; set; } = "";

    public async Task<IActionResult> OnGet([FromQuery] string? redirect)
    {
        WebToken? token = this.Database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        this.RedirectUrl = redirect ?? "~/";

        if (token.Verified) return this.Redirect(this.RedirectUrl);

        User? user = await this.Database.Users.Where(u => u.UserId == token.UserId).FirstOrDefaultAsync();
        if (user == null) return this.Redirect("~/login");

        if (!user.IsTwoFactorSetup) return this.Redirect(this.RedirectUrl);

        return this.Page();
    }

    public async Task<IActionResult> OnPost([FromForm] string? code, [FromForm] string? redirect)
    {

        WebToken? token = this.Database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        this.RedirectUrl = redirect ?? "~/";

        if (token.Verified) return this.Redirect(this.RedirectUrl);

        User? user = await this.Database.Users.Where(u => u.UserId == token.UserId).FirstOrDefaultAsync();
        if (user == null) return this.Redirect("~/login");

        if (!user.IsTwoFactorSetup)
        {
            token.Verified = true;
            await this.Database.SaveChangesAsync();
        }

        if (CryptoHelper.VerifyCode(code, user.TwoFactorSecret, user.TwoFactorBackup))
        {
            token.Verified = true;
            await this.Database.SaveChangesAsync();

            return this.Redirect(this.RedirectUrl);
        }

        this.Error = this.Translate(code?.Length == 8 ? TwoFactorStrings.InvalidCode : TwoFactorStrings.InvalidBackupCode);
        return this.Page();
    }
}