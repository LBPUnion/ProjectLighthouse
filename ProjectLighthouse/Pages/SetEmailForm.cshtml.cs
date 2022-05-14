#nullable enable
using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Profiles.Email;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages;

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
        };

        this.Database.EmailVerificationTokens.Add(emailVerifyToken);

        // The user just set their email address. Now, let's grant them a token to proceed with verifying the email.
        WebToken webToken = new()
        {
            UserId = user.UserId,
            UserToken = CryptoHelper.GenerateAuthToken(),
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

        Logger.LogSuccess($"User {user.Username} (id: {user.UserId}) successfully logged in on web after setting an email address", LogArea.Login);

        this.Database.WebTokens.Add(webToken);
        await this.Database.SaveChangesAsync();

        return this.Redirect("/login/sendVerificationEmail");
    }
}