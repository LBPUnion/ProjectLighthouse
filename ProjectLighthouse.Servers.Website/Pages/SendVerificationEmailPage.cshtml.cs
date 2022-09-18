#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles.Email;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class SendVerificationEmailPage : BaseLayout
{
    public SendVerificationEmailPage(Database database) : base(database)
    {}

    public bool Success { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (!ServerConfiguration.Instance.Mail.MailEnabled) return this.NotFound();

        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");

        // `using` weirdness here. I tried to fix it, but I couldn't.
        // The user should never see this page once they've been verified, so assert here.
        System.Diagnostics.Debug.Assert(!user.EmailAddressVerified);

        // Othewise, on a release build, just silently redirect them to the landing page.
        #if !DEBUG
        if (user.EmailAddressVerified)
        {
            return this.Redirect("/");
        }
        #endif

        EmailVerificationToken? verifyToken = await this.Database.EmailVerificationTokens.FirstOrDefaultAsync(v => v.UserId == user.UserId); 
        // If user doesn't have a token or it is expired then regenerate
        if (verifyToken == null || DateTime.Now > verifyToken.ExpiresAt)
        {
            verifyToken = new EmailVerificationToken
            {
                UserId = user.UserId,
                User = user,
                EmailToken = CryptoHelper.GenerateAuthToken(),
                ExpiresAt = DateTime.Now.AddHours(6),
            };

            this.Database.EmailVerificationTokens.Add(verifyToken);
            await this.Database.SaveChangesAsync();
        }

        string body = "Hello,\n\n" +
                      $"This email is a request to verify this email for your (likely new!) Project Lighthouse account ({user.Username}).\n\n" +
                      $"To verify your account, click the following link: {ServerConfiguration.Instance.ExternalUrl}/verifyEmail?token={verifyToken.EmailToken}\n\n\n" +
                      "If this wasn't you, feel free to ignore this email.";

        this.Success = SMTPHelper.SendEmail(user.EmailAddress, "Project Lighthouse Email Verification", body);

        return this.Page();
    }
}