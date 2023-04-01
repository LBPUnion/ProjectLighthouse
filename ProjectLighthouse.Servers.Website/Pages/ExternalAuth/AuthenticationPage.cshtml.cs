#nullable enable
using System.Net;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.ExternalAuth;

public class AuthenticationPage : BaseLayout
{

    public List<PlatformLinkAttemptEntity> LinkAttempts = new();

    public IPAddress? IpAddress;
    public AuthenticationPage(DatabaseContext database) : base(database)
    {}

    public IActionResult OnGet()
    {
        if (this.User == null) return this.Redirect("~/login");

        this.IpAddress = this.HttpContext.Connection.RemoteIpAddress;

        this.LinkAttempts = this.Database.PlatformLinkAttempts
        .Where(l => l.UserId == this.User.UserId)
        .OrderByDescending(a => a.Timestamp)
        .ToList();

        return this.Page();
    }
}