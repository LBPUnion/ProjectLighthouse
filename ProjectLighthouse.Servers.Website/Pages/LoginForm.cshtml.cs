#nullable enable
using System.Web;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Localization.StringLists;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles.Email;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class LoginForm : BaseLayout
{
    public LoginForm(Database database) : base(database)
    {}

    public string? Error { get; private set; }

    [UsedImplicitly]
    public async Task<IActionResult> OnPost(string username, string password, string redirect)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            this.Error = ServerConfiguration.Instance.Mail.MailEnabled ? this.Translate(ErrorStrings.UsernameInvalid) : this.Translate(ErrorStrings.EmailInvalid);
            return this.Page();
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            this.Error = this.Translate(ErrorStrings.PasswordInvalid);
            return this.Page();
        }

        if (!await this.Request.CheckCaptchaValidity())
        {
            this.Error = this.Translate(ErrorStrings.CaptchaFailed);
            return this.Page();
        }

        User? user;

        if (!ServerConfiguration.Instance.Mail.MailEnabled)
        {
            user = await this.Database.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
        else
        {
            user = await this.Database.Users.FirstOrDefaultAsync(u => u.EmailAddress == username);
            if (user == null)
            {
                User? noEmailUser = await this.Database.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (noEmailUser != null && noEmailUser.EmailAddress == null) user = noEmailUser;

            }
        }

        if (user == null)
        {
            Logger.Warn($"User {username} failed to login on web due to invalid username", LogArea.Login);
            this.Error = ServerConfiguration.Instance.Mail.MailEnabled
                ? "The email or password you entered is invalid."
                : "The username or password you entered is invalid.";
            return this.Page();
        }

        if (!BCrypt.Net.BCrypt.Verify(password, user.Password))
        {
            Logger.Warn($"User {user.Username} (id: {user.UserId}) failed to login on web due to invalid password", LogArea.Login);
            this.Error = ServerConfiguration.Instance.Mail.MailEnabled
                ? "The email or password you entered is invalid."
                : "The username or password you entered is invalid.";
            return this.Page();
        }

        if (user.IsBanned)
        {
            Logger.Warn($"User {user.Username} (id: {user.UserId}) failed to login on web due to being banned", LogArea.Login);
            this.Error = this.Translate(ErrorStrings.UserIsBanned, user.BannedReason);
            return this.Page();
        }

        if (user.EmailAddress == null && ServerConfiguration.Instance.Mail.MailEnabled)
        {
            Logger.Warn($"User {user.Username} (id: {user.UserId}) failed to login; email not set", LogArea.Login);

            EmailSetToken emailSetToken = new()
            {
                UserId = user.UserId,
                User = user,
                EmailToken = CryptoHelper.GenerateAuthToken(),
                ExpiresAt = DateTime.Now + TimeSpan.FromHours(6),
            };

            this.Database.EmailSetTokens.Add(emailSetToken);
            await this.Database.SaveChangesAsync();

            return this.Redirect("/login/setEmail?token=" + emailSetToken.EmailToken);
        }

        WebToken webToken = new()
        {
            UserId = user.UserId,
            UserToken = CryptoHelper.GenerateAuthToken(),
            ExpiresAt = DateTime.Now + TimeSpan.FromDays(7),
            Verified = !ServerConfiguration.Instance.TwoFactorConfiguration.TwoFactorEnabled || !user.IsTwoFactorSetup,
        };

        this.Database.WebTokens.Add(webToken);
        await this.Database.SaveChangesAsync();

        this.Response.Cookies.Append
        (
            "LighthouseToken",
            webToken.UserToken,
            new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
            }
        );

        Logger.Success($"User {user.Username} (id: {user.UserId}) successfully logged in on web", LogArea.Login);

        if (user.PasswordResetRequired) return this.Redirect("~/passwordResetRequired");
        if (ServerConfiguration.Instance.Mail.MailEnabled && !user.EmailAddressVerified) return this.Redirect("~/login/sendVerificationEmail");

        if (!webToken.Verified)
        {
            return string.IsNullOrWhiteSpace(redirect)
                ? this.Redirect("~/2fa")
                : this.Redirect("~/2fa" + "?redirect=" + HttpUtility.UrlEncode(redirect));
        }


        if (string.IsNullOrWhiteSpace(redirect))
        {
            return this.RedirectToPage(nameof(LandingPage));
        }
        return this.Redirect(redirect);
    }

    [UsedImplicitly]
    public IActionResult OnGet()
    {
        if (this.Database.UserFromWebRequest(this.Request) != null) 
            return this.RedirectToPage(nameof(LandingPage));

        return this.Page();
    }
}