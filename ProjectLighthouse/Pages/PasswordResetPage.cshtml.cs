#nullable enable
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages;

public class PasswordResetPage : BaseLayout
{
    public PasswordResetPage(Database database) : base(database)
    {}

    public string? Error { get; private set; }

    [UsedImplicitly]
    public async Task<IActionResult> OnPost(string password, string confirmPassword)
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

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

        if (!user.EmailAddressVerified) return this.Redirect("~/login/sendVerificationEmail");

        return this.Redirect("~/");
    }

    [UsedImplicitly]
    public IActionResult OnGet()
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        return this.Page();
    }
}