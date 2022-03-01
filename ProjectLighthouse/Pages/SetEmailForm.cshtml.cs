#nullable enable
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Profiles.Email;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages;

public class SetEmailForm : BaseLayout
{
    public SetEmailForm(Database database) : base(database)
    {}

    public EmailSetToken EmailToken;

    public async Task<IActionResult> OnGet(string? token = null)
    {
        if (token == null) return this.Redirect("/login");

        EmailSetToken? emailToken = await this.Database.EmailSetTokens.FirstOrDefaultAsync(t => t.EmailToken == token);
        if (emailToken == null) return this.Redirect("/login");

        this.EmailToken = emailToken;

        return this.Page();
    }

    public async Task<IActionResult> OnPost(string emailAddress, string token)
    {
        EmailSetToken? emailToken = await this.Database.EmailSetTokens.Include(t => t.User).FirstOrDefaultAsync(t => t.EmailToken == token);
        if (emailToken == null) return this.Redirect("/login");

        emailToken.User.EmailAddress = emailAddress;
        this.Database.EmailSetTokens.Remove(emailToken);

        EmailVerificationToken emailVerifyToken = new()
        {
            UserId = emailToken.UserId,
            User = emailToken.User,
            EmailToken = HashHelper.GenerateAuthToken(),
        };

        this.Database.EmailVerificationTokens.Add(emailVerifyToken);

        await this.Database.SaveChangesAsync();

        return this.Redirect("/login/verify?token=" + emailVerifyToken.EmailToken);
    }
}