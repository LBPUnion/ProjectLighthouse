#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles.Email;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Email;

public class CompleteEmailVerificationPage : BaseLayout
{
    public CompleteEmailVerificationPage(Database database) : base(database)
    {}

    public string? Error;

    public async Task<IActionResult> OnGet(string token)
    {
        if (!ServerConfiguration.Instance.Mail.MailEnabled) return this.NotFound();

        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        EmailVerificationToken? emailVerifyToken = await this.Database.EmailVerificationTokens.FirstOrDefaultAsync(e => e.EmailToken == token);
        if (emailVerifyToken == null)
        {
            this.Error = "Invalid verification token";
            return this.Page();
        }

        if (DateTime.Now > emailVerifyToken.ExpiresAt)
        {
            this.Error = "This token has expired";
            return this.Page();
        }

        if (emailVerifyToken.UserId != user.UserId)
        {
            this.Error = "This token doesn't belong to you!";
            return this.Page();
        }

        this.Database.EmailVerificationTokens.Remove(emailVerifyToken);

        user.EmailAddressVerified = true;

        await this.Database.SaveChangesAsync();

        return this.Page();
    }
}