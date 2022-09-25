#nullable enable
using System.Collections.Concurrent;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
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

    // (User id, timestamp of last request + 30 seconds)
    private static readonly ConcurrentDictionary<int, long> recentlySentEmail = new();

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

        // Remove expired entries
        for (int i = recentlySentEmail.Count - 1; i >= 0; i--)
        {
            KeyValuePair<int, long> entry = recentlySentEmail.ElementAt(i);
            bool valueExists = recentlySentEmail.TryGetValue(entry.Key, out long timestamp);
            if (!valueExists)
            {
                recentlySentEmail.TryRemove(entry.Key, out _);
                continue;
            }
            if (TimeHelper.TimestampMillis > timestamp) recentlySentEmail.TryRemove(entry.Key, out _);
        }


        if (recentlySentEmail.ContainsKey(user.UserId))
        {
            bool valueExists = recentlySentEmail.TryGetValue(user.UserId, out long timestamp);
            if (!valueExists)
            {
                recentlySentEmail.TryRemove(user.UserId, out _);
            } 
            else if (timestamp > TimeHelper.TimestampMillis)
            {
                this.Success = true;
                return this.Page();
            }
        }

        string? existingToken = await this.Database.EmailVerificationTokens.Where(v => v.UserId == user.UserId).Select(v => v.EmailToken).FirstOrDefaultAsync();
        if(existingToken != null)
            this.Database.EmailVerificationTokens.RemoveWhere(t => t.EmailToken == existingToken);

        EmailVerificationToken verifyToken = new()
        {
            UserId = user.UserId,
            User = user,
            EmailToken = CryptoHelper.GenerateAuthToken(),
            ExpiresAt = DateTime.Now.AddHours(6),
        };

        this.Database.EmailVerificationTokens.Add(verifyToken);
        await this.Database.SaveChangesAsync();

        string body = "Hello,\n\n" +
                      $"This email is a request to verify this email for your (likely new!) Project Lighthouse account ({user.Username}).\n\n" +
                      $"To verify your account, click the following link: {ServerConfiguration.Instance.ExternalUrl}/verifyEmail?token={verifyToken.EmailToken}\n\n\n" +
                      "If this wasn't you, feel free to ignore this email.";

        this.Success = SMTPHelper.SendEmail(user.EmailAddress, "Project Lighthouse Email Verification", body);

        // Don't send another email for 30 seconds
        recentlySentEmail.TryAdd(user.UserId, TimeHelper.TimestampMillis + 30 * 1000);

        return this.Page();
    }
}