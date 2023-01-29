#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Entities.Profile;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Email;

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

        if (user.EmailAddressVerified) return this.Redirect("/");

        this.Success = await SMTPHelper.SendVerificationEmail(this.Database, user);

        return this.Page();
    }
}