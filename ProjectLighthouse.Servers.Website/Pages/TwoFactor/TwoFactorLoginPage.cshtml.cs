using LBPUnion.ProjectLighthouse.Configuration;
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
        if (!ServerConfiguration.Instance.TwoFactorConfiguration.TwoFactorEnabled) return this.Redirect("~/login");

        WebToken? token = this.Database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        this.RedirectUrl = redirect ?? "~/";

        if (token.Verified) return this.Redirect(this.RedirectUrl);

        User? user = await this.Database.Users.Where(u => u.UserId == token.UserId).FirstOrDefaultAsync();
        if (user == null) return this.Redirect("~/login");

        if (user.IsTwoFactorSetup) return this.Page();

        token.Verified = true;
        await this.Database.SaveChangesAsync();
        return this.Redirect(this.RedirectUrl);
    }

    public async Task<IActionResult> OnPost([FromForm] string? code, [FromForm] string? redirect, [FromForm] string? backup)
    {
        if (!ServerConfiguration.Instance.TwoFactorConfiguration.TwoFactorEnabled) return this.Redirect("~/login");

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
            return this.Redirect(this.RedirectUrl);
        }

        // if both are null or neither are null, there should only be one at at time
        if (string.IsNullOrWhiteSpace(code) == string.IsNullOrWhiteSpace(backup))
        {
            this.Error = this.Translate(TwoFactorStrings.InvalidCode);
            return this.Page();
        }

        if (string.IsNullOrWhiteSpace(backup))
        {
            if (!CryptoHelper.VerifyCode(code, user.TwoFactorSecret))
            {
                this.Error = this.Translate(TwoFactorStrings.InvalidCode);
                return this.Page();
            }
        }
        else
        {
            if (!CryptoHelper.VerifyBackup(backup, user.TwoFactorBackup))
            {
                this.Error = this.Translate(TwoFactorStrings.InvalidBackupCode);
                return this.Page();
            }
        }

        token.Verified = true;
        await this.Database.SaveChangesAsync();

        return this.Redirect(this.RedirectUrl);
    }
}