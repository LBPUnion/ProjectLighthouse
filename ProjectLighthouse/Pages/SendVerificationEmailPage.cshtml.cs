#nullable enable
using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Profiles.Email;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages;

public class SendVerificationEmailPage : BaseLayout
{
    public SendVerificationEmailPage(Database database) : base(database)
    {}

    public async Task<IActionResult> OnGet()
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");

        EmailVerificationToken verifyToken = new()
        {
            UserId = user.UserId,
            User = user,
            EmailToken = HashHelper.GenerateAuthToken(),
        };

        this.Database.EmailVerificationTokens.Add(verifyToken);

        await this.Database.SaveChangesAsync();

        string body = "Hello,\n\n" +
                      $"A request to verify this email for your Project Lighthouse account ({user.Username}).\n\n" +
                      $"To verify your account, click this link: {ServerSettings.Instance.ExternalUrl}/verifyEmail?token={verifyToken.EmailToken}";

        if (SMTPHelper.SendEmail(user.EmailAddress, "Project Lighthouse Email Verification", body))
        {
            return this.Page();
        }
        else
        {
            throw new Exception("failed to send email");
        }
    }
}