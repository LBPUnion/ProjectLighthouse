using System.Diagnostics.CodeAnalysis;
using System.Net;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Localization.StringLists;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class RegisterForm : BaseLayout
{
    public RegisterForm(Database database) : base(database)
    { }

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

        //TODO rework registration
        User? existingUser = await this.Database.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower() && u.Password == null);
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

        if (!await this.Request.CheckCaptchaValidity())
        {
            this.Error = this.Translate(ErrorStrings.CaptchaFailed);
            return this.Page();
        }

        User user = await this.Database.CreateUser(username, CryptoHelper.BCryptHash(password), emailAddress);
        if (existingUser != null)
        {
            user.Password = CryptoHelper.BCryptHash(password);
            user.EmailAddress = emailAddress;
        }

        WebToken webToken = new()
        {
            UserId = user.UserId,
            UserToken = CryptoHelper.GenerateAuthToken(),
            ExpiresAt = DateTime.Now + TimeSpan.FromDays(7),
        };

        this.Database.WebTokens.Add(webToken);
        await this.Database.SaveChangesAsync();

        this.Response.Cookies.Append("LighthouseToken", webToken.UserToken);

        if (ServerConfiguration.Instance.Mail.MailEnabled) return this.Redirect("~/login/sendVerificationEmail");

        return this.RedirectToPage(nameof(LandingPage));
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