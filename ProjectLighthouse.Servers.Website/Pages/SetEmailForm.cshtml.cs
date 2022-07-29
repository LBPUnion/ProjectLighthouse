#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles.Email;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class SetEmailForm : BaseLayout
{
    public SetEmailForm(Database database) : base(database)
    {}

    public EmailSetToken? EmailToken;

    public async Task<IActionResult> OnGet(string? token = null)
    {
        if (!ServerConfiguration.Instance.Mail.MailEnabled) return this.NotFound();
        if (token == null) return this.Redirect("/login");

        EmailSetToken? emailToken = await this.Database.EmailSetTokens.FirstOrDefaultAsync(t => t.EmailToken == token);
        if (emailToken == null) return this.Redirect("/login");

        this.EmailToken = emailToken;

        return this.Page();
    }

    public async Task<IActionResult> OnPost(string emailAddress, string token)
    {
        if (!ServerConfiguration.Instance.Mail.MailEnabled) return this.NotFound();

        EmailSetToken? emailToken = await this.Database.EmailSetTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.EmailToken == token);
        if (emailToken == null) return this.Redirect("/login");

        emailToken.User.EmailAddress = emailAddress;
        this.Database.EmailSetTokens.Remove(emailToken);

        User user = emailToken.User;

        EmailVerificationToken emailVerifyToken = new()
        {
            UserId = user.UserId,
            User = user,
            EmailToken = CryptoHelper.GenerateAuthToken(),
            ExpiresAt = DateTime.Now + TimeSpan.FromHours(6),
        };

        this.Database.EmailVerificationTokens.Add(emailVerifyToken);

        // The user just set their email address. Now, let's grant them a token to proceed with verifying the email.
        // TODO: insecure
        WebToken webToken = new()
        {
            UserId = user.UserId,
            UserToken = CryptoHelper.GenerateAuthToken(),
            ExpiresAt = DateTime.Now + TimeSpan.FromDays(7),
        };

        this.Response.Cookies.Append
        (
            "LighthouseToken",
            webToken.UserToken,
            new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
            }
        );

        Logger.Success($"User {user.Username} (id: {user.UserId}) successfully logged in on web after setting an email address", LogArea.Login);

        this.Database.WebTokens.Add(webToken);
        await this.Database.SaveChangesAsync();

        return this.Redirect("/login/sendVerificationEmail");
    }
}