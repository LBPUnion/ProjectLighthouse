#nullable enable
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class PasswordResetPage : BaseLayout
{
    public PasswordResetPage(Database database) : base(database)
    {}

    public string? Error { get; private set; }

    [UsedImplicitly]
    public async Task<IActionResult> OnPost(string password, string confirmPassword)
    {
        User? user;
        if (this.Request.Query.ContainsKey("token"))
        {
            user = await this.Database.UserFromPasswordResetToken(this.Request.Query["token"][0]);
            if (user == null)
            {
                this.Error = "This password reset link either is invalid or has expired. Please try again.";
                return this.Page();
            }
        }
        else
        {
            user = this.Database.UserFromWebRequest(this.Request);
            if (user == null) return this.Redirect("~/login");
        }

        if (string.IsNullOrWhiteSpace(password))
        {
            this.Error = "The password field is required.";
            return this.Page();
        }

        if (password != confirmPassword)
        {
            this.Error = "Passwords do not match!";
            return this.Page();
        }

        user.Password = CryptoHelper.BCryptHash(password);
        user.PasswordResetRequired = false;

        await this.Database.SaveChangesAsync();

        if (!user.EmailAddressVerified && ServerConfiguration.Instance.Mail.MailEnabled) 
            return this.Redirect("~/login/sendVerificationEmail");

        return this.Redirect("~/");
    }

    [UsedImplicitly]
    public IActionResult OnGet()
    {
        if (this.Request.Query.ContainsKey("token")) return this.Page();
        
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        return this.Page();
    }
}