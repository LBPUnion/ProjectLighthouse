#nullable enable
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Profiles.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages;

public class CompleteEmailVerificationPage : BaseLayout
{
    public CompleteEmailVerificationPage([NotNull] Database database) : base(database)
    {}

    public string? Error = null;

    public async Task<IActionResult> OnGet(string token)
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        EmailVerificationToken? emailVerifyToken = await this.Database.EmailVerificationTokens.FirstOrDefaultAsync(e => e.EmailToken == token);
        if (emailVerifyToken == null)
        {
            this.Error = "Invalid verification token";
            return this.Page();
        }

        this.Database.EmailVerificationTokens.Remove(emailVerifyToken);

        user.EmailAddressVerified = true;

        await this.Database.SaveChangesAsync();

        return this.Page();
    }
}