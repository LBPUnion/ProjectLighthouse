using System.Diagnostics.CodeAnalysis;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Localization.StringLists;
using LBPUnion.ProjectLighthouse.Servers.Website.Captcha;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Login;

public class RegisterForm : BaseLayout
{
    public readonly IMailService Mail;
    private readonly ICaptchaService captchaService;

    public RegisterForm(DatabaseContext database, IMailService mail, ICaptchaService captchaService) : base(database)
    {
        this.Mail = mail;
        this.captchaService = captchaService;
    }

    public string? Error { get; private set; }

    public string? Username { get; set; }

    [UsedImplicitly]
    [SuppressMessage("ReSharper", "SpecifyStringComparison")]
    public async Task<IActionResult> OnPost(string username, string password, string confirmPassword, string emailAddress)
    {
        if (this.Database.UserFromWebRequest(this.Request) != null) return this.Redirect("~/");

        if (!ServerConfiguration.Instance.Authentication.RegistrationEnabled) return this.NotFound();

        if (string.IsNullOrWhiteSpace(username))
        {
            this.Error = this.Translate(ErrorStrings.UsernameInvalid);
            return this.Page();
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            this.Error = this.Translate(ErrorStrings.PasswordInvalid);
            return this.Page();
        }

        if (string.IsNullOrWhiteSpace(emailAddress) && ServerConfiguration.Instance.Mail.MailEnabled)
        {
            this.Error = this.Translate(ErrorStrings.EmailInvalid);
            return this.Page();
        }

        if (password != confirmPassword)
        {
            this.Error = this.Translate(ErrorStrings.PasswordDoesntMatch);
            return this.Page();
        }

        UserEntity? existingUser = await this.Database.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower());
        if (existingUser != null)
        {
            this.Error = this.Translate(ErrorStrings.UsernameTaken);
            return this.Page();
        }

        if (ServerConfiguration.Instance.Mail.MailEnabled &&
            await this.Database.Users.AnyAsync(u => u.EmailAddress != null && u.EmailAddress.ToLower() == emailAddress.ToLower()))
        {
            this.Error = this.Translate(ErrorStrings.EmailTaken);
            return this.Page();
        }

        if (!await this.captchaService.VerifyCaptcha(this.Request))
        {
            this.Error = this.Translate(ErrorStrings.CaptchaFailed);
            return this.Page();
        }

        UserEntity user = await this.Database.CreateUser(username, CryptoHelper.BCryptHash(password), emailAddress);

        if(ServerConfiguration.Instance.Mail.MailEnabled) SMTPHelper.SendRegistrationEmail(this.Mail, user);

        WebTokenEntity webToken = new()
        {
            UserId = user.UserId,
            UserToken = CryptoHelper.GenerateAuthToken(),
            ExpiresAt = DateTime.Now + TimeSpan.FromDays(7),
        };

        this.Database.WebTokens.Add(webToken);
        await this.Database.SaveChangesAsync();

        this.Response.Cookies.Append("LighthouseToken", webToken.UserToken);

        return ServerConfiguration.Instance.Mail.MailEnabled ? 
            this.Redirect("~/login/sendVerificationEmail") : 
            this.Redirect("~/");
    }

    [UsedImplicitly]
    [SuppressMessage("ReSharper", "SpecifyStringComparison")]
    public IActionResult OnGet()
    {
        this.Error = string.Empty;
        if (!ServerConfiguration.Instance.Authentication.RegistrationEnabled)
        {
            return this.NotFound();
        }

        return this.Page();
    }
}