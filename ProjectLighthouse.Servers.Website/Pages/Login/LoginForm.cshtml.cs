#nullable enable
using System.Web;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Localization.StringLists;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Servers.Website.Captcha;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Logging;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Login;

public class LoginForm : BaseLayout
{
    private readonly ICaptchaService captchaService;

    public LoginForm(DatabaseContext database, ICaptchaService captchaService) : base(database)
    {
        this.captchaService = captchaService;
    }

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

        if (!await this.captchaService.VerifyCaptcha(this.Request))
        {
            this.Error = this.Translate(ErrorStrings.CaptchaFailed);
            return this.Page();
        }

        UserEntity? user;

        if (!ServerConfiguration.Instance.Mail.MailEnabled)
        {
            user = await this.Database.Users.FirstOrDefaultAsync(u => u.Username == username);
        }
        else
        {
            user = await this.Database.Users.FirstOrDefaultAsync(u => u.EmailAddress == username);
            if (user == null)
            {
                UserEntity? noEmailUser = await this.Database.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (noEmailUser != null && noEmailUser.EmailAddress == null) user = noEmailUser;

            }
        }

        if (user == null || user.Password == null)
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

        WebTokenEntity webToken = new()
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

        if (!webToken.Verified)
        {
            return string.IsNullOrWhiteSpace(redirect)
                ? this.Redirect("~/2fa")
                : this.Redirect("~/2fa" + "?redirect=" + HttpUtility.UrlEncode(redirect));
        }

        if (user.PasswordResetRequired) return this.Redirect("~/passwordResetRequired");

        return ServerConfiguration.Instance.Mail.MailEnabled switch
        {
            true when string.IsNullOrWhiteSpace(user.EmailAddress) => this.Redirect("~/login/setEmail"),
            true when !user.EmailAddressVerified => this.Redirect("~/login/sendVerificationEmail"),
            _ => string.IsNullOrWhiteSpace(redirect) ? this.Redirect("~/") : this.Redirect(redirect),
        };
    }

    [UsedImplicitly]
    public IActionResult OnGet()
    {
        if (this.Database.UserFromWebRequest(this.Request) != null) return this.Redirect("~/");

        return this.Page();
    }
}