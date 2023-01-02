#nullable enable
using System.Diagnostics.CodeAnalysis;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Localization.StringLists;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Email;

public class SetEmailForm : BaseLayout
{
    public SetEmailForm(Database database) : base(database)
    {}

    public string? Error { get; private set; }

    public IActionResult OnGet()
    {
        if (!ServerConfiguration.Instance.Mail.MailEnabled) return this.NotFound();
        WebToken? token = this.Database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("/login");

        return this.Page();
    }

    [SuppressMessage("ReSharper", "SpecifyStringComparison")]
    public async Task<IActionResult> OnPost(string emailAddress)
    {
        if (!ServerConfiguration.Instance.Mail.MailEnabled) return this.NotFound();

        WebToken? token = this.Database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        User? user = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == token.UserId);
        if (user == null) return this.Redirect("~/login");

        if (!SanitizationHelper.IsValidEmail(emailAddress))
        {
            this.Error = this.Translate(ErrorStrings.EmailInvalid);
            return this.Page();
        }

        if (await this.Database.Users.AnyAsync(u => u.EmailAddress != null && u.EmailAddress.ToLower() == emailAddress.ToLower()))
        {
            this.Error = this.Translate(ErrorStrings.EmailTaken);
            return this.Page();
        }

        user.EmailAddress = emailAddress;
        await this.Database.SaveChangesAsync();

        return this.Redirect("/login/sendVerificationEmail");
    }
}