using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Localization.StringLists;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.TwoFactor;

public class DisableTwoFactorPage : BaseLayout
{
    public DisableTwoFactorPage(Database database) : base(database) { }

    public string Error { get; set; } = "";

    public IActionResult OnGet()
    {
        if (!ServerConfiguration.Instance.TwoFactorConfiguration.TwoFactorEnabled) return this.Redirect("~/login");

        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        if (!user.IsTwoFactorSetup) return this.Redirect("~/user/" + user.UserId + "/settings");

        return this.Page();
    }

    public async Task<IActionResult> OnPost([FromForm] string? code, [FromForm] string? backup)
    {
        if (!ServerConfiguration.Instance.TwoFactorConfiguration.TwoFactorEnabled) return this.Redirect("~/login");

        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        if (!user.IsTwoFactorSetup) return this.Redirect("~/user/" + user.UserId + "/settings");

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
            if(!CryptoHelper.VerifyBackup(backup, user.TwoFactorBackup))
            {
                this.Error = this.Translate(TwoFactorStrings.InvalidBackupCode);
                return this.Page();
            }
        }

        user.TwoFactorBackup = null;
        user.TwoFactorSecret = null;
        await this.Database.SaveChangesAsync();

        return this.Redirect("~/user/" + user.UserId + "/settings");
    }
}