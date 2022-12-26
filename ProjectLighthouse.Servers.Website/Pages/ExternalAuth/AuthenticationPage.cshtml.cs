#nullable enable
using System.Net;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.ExternalAuth;

public class AuthenticationPage : BaseLayout
{

    public List<PlatformLinkAttempt> LinkAttempts = new();

    public IPAddress? IpAddress;
    public AuthenticationPage(Database database) : base(database)
    {}

    public IActionResult OnGet()
    {
        if (this.User == null) return this.StatusCode(403, "");

        this.IpAddress = this.HttpContext.Connection.RemoteIpAddress;

        this.LinkAttempts = this.Database.PlatformLinkAttempts
        .Where(l => l.UserId == this.User.UserId)
        .OrderByDescending(a => a.Timestamp)
        .ToList();

        return this.Page();
    }
}