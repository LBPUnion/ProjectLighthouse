using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Mail;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Login;

public class PasswordResetRequestForm : BaseLayout
{
    public IMailService Mail;

    public string? Error { get; private set; }

    public string? Status { get; private set; }

    public PasswordResetRequestForm(DatabaseContext database, IMailService mail) : base(database)
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

        await SMTPHelper.SendPasswordResetEmail(this.Database, this.Mail, user);

        this.Status = $"A password reset request has been sent to the email {email}. " +
                      "If you do not receive an email verify that you have entered the correct email address";
        return this.Page();
    }
    public void OnGet() => this.Page();
}