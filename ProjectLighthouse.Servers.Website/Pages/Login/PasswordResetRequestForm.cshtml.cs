using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Mail;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Login;

public class PasswordResetRequestForm : BaseLayout
{

    public MailQueueService Mail;

    public string? Error { get; private set; }

    public string? Status { get; private set; }

    public PasswordResetRequestForm(DatabaseContext database, MailQueueService mail) : base(database)
    {
        this.Mail = mail;
    }

    [UsedImplicitly]
    public async Task<IActionResult> OnPost(string email)
    {

        if (!ServerConfiguration.Instance.Mail.MailEnabled)
        {
            this.Error = "Email is not configured on this server, so password resets cannot be issued. Please contact your instance administrator for more details.";
            return this.Page();
        }

        if (string.IsNullOrWhiteSpace(email))
        {
            this.Error = "The email field is required.";
            return this.Page();
        }

        if (!SanitizationHelper.IsValidEmail(email))
        {
            this.Error = "This email is in an invalid format";
            return this.Page();
        }

        UserEntity? user = await this.Database.Users.FirstOrDefaultAsync(u => u.EmailAddress == email && u.EmailAddressVerified);

        if (user == null)
        {
            this.Status = $"A password reset request has been sent to the email {email}. " +
                          "If you do not receive an email verify that you have entered the correct email address";
            return this.Page();
        }

        PasswordResetTokenEntity token = new()
        {
            Created = DateTime.Now,
            UserId = user.UserId,
            ResetToken = CryptoHelper.GenerateAuthToken(),
        };

        string messageBody = $"Hello, {user.Username}.\n\n" +
            "A request to reset your account's password was issued. If this wasn't you, this can probably be ignored.\n\n" +
            $"If this was you, your {ServerConfiguration.Instance.Customization.ServerName} password can be reset at the following link:\n" +
            $"{ServerConfiguration.Instance.ExternalUrl}/passwordReset?token={token.ResetToken}";

        //TODO refactor to use EmailHelper to have cooldown
        this.Mail.SendEmail(user.EmailAddress, $"Project Lighthouse Password Reset Request for {user.Username}", messageBody);

        this.Database.PasswordResetTokens.Add(token);
        await this.Database.SaveChangesAsync();

        this.Status = $"A password reset request has been sent to the email {email}. " +
                      "If you do not receive an email verify that you have entered the correct email address";
        return this.Page();
    }
    public void OnGet() => this.Page();
}