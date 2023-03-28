#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Email;

public class CompleteEmailVerificationPage : BaseLayout
{
    public CompleteEmailVerificationPage(DatabaseContext database) : base(database)
    {}

    public string? Error;

    public async Task<IActionResult> OnGet(string token)
    {
        if (!ServerConfiguration.Instance.Mail.MailEnabled) return this.NotFound();

        EmailVerificationTokenEntity? emailVerifyToken = await this.Database.EmailVerificationTokens.FirstOrDefaultAsync(e => e.EmailToken == token);
        if (emailVerifyToken == null)
        {
            this.Error = "Invalid verification token";
            return this.Page();
        }

        UserEntity user = await this.Database.Users.FirstAsync(u => u.UserId == emailVerifyToken.UserId);

        if (DateTime.Now > emailVerifyToken.ExpiresAt)
        {
            this.Error = "This token has expired";
            return this.Page();
        }

        if (emailVerifyToken.UserId != user.UserId)
        {
            this.Error = "This token doesn't belong to you!";
            return this.Page();
        }

        this.Database.EmailVerificationTokens.Remove(emailVerifyToken);

        user.EmailAddressVerified = true;
        await this.Database.SaveChangesAsync();

        if (user.Password != null) return this.Page();

        // if user's account was created automatically
        WebTokenEntity webToken = new()
        {
            ExpiresAt = DateTime.Now.AddDays(7),
            Verified = true,
            UserId = user.UserId,
            UserToken = CryptoHelper.GenerateAuthToken(),
        };
        user.PasswordResetRequired = true;
        this.Database.WebTokens.Add(webToken);
        await this.Database.SaveChangesAsync();
        this.Response.Cookies.Append("LighthouseToken",
            webToken.UserToken,
            new CookieOptions
            {
                Expires = DateTimeOffset.Now.AddDays(7),
            });
        return this.Redirect("/passwordReset");
    }
}