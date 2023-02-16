#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Email;

public class SendVerificationEmailPage : BaseLayout
{
    public SendVerificationEmailPage(DatabaseContext database) : base(database)
    {}

    public bool Success { get; set; }

    public async Task<IActionResult> OnGet()
    {
        if (!ServerConfiguration.Instance.Mail.MailEnabled) return this.NotFound();

        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");

        if (user.EmailAddressVerified) return this.Redirect("/");

        this.Success = await SMTPHelper.SendVerificationEmail(this.Database, user);

        return this.Page();
    }
}