using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class PasswordResetRequestForm : BaseLayout
{

    public string? Error { get; private set; }

    public string? Status { get; private set; }

    public PasswordResetRequestForm(Database database) : base(database)
    { }

    [UsedImplicitly]
    public async Task<IActionResult> OnPost(string username)
    {

        if (!ServerConfiguration.Instance.Mail.MailEnabled)
        {
            this.Error = "Email is not configured on this server, so password resets cannot be issued. Please contact your instance administrator for more details.";
            return this.Page();
        }

        if (string.IsNullOrWhiteSpace(username))
        {
            this.Error = "The username field is required.";
            return this.Page();
        }

        User? user = await this.Database.Users.FirstOrDefaultAsync(u => u.Username == username);

        if (user == null)
        {
            this.Error = "User does not exist.";
            return this.Page();
        }

        PasswordResetToken token = new()
        {
            Created = DateTime.Now,
            UserId = user.UserId,
            ResetToken = CryptoHelper.GenerateAuthToken(),
        };

        string messageBody = $"Hello, {user.Username}.\n\n" +
            "A request to reset your account's password was issued. If this wasn't you, this can probably be ignored.\n\n" +
            $"If this was you, your {ServerConfiguration.Instance.Customization.ServerName} password can be reset at the following link:\n" +
            $"{ServerConfiguration.Instance.ExternalUrl}/passwordReset?token={token.ResetToken}";

        SMTPHelper.SendEmail(user.EmailAddress, $"Project Lighthouse Password Reset Request for {user.Username}", messageBody);

        this.Database.PasswordResetTokens.Add(token);
        await this.Database.SaveChangesAsync();
        
        this.Status = $"Password reset email sent to {CensorHelper.MaskEmail(user.EmailAddress)}.";
        return this.Page();
    }
    public void OnGet() => this.Page();
}