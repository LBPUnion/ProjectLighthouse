#nullable enable
using System.Net;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Settings;
using LBPUnion.ProjectLighthouse.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Website.Pages.ExternalAuth;

public class AuthenticationPage : BaseLayout
{

    public List<AuthenticationAttempt> AuthenticationAttempts = new();

    public IPAddress? IpAddress;
    public AuthenticationPage(Database database) : base(database)
    {}

    public IActionResult OnGet()
    {
        if (!ServerConfiguration.Instance.Authentication.UseExternalAuth) return this.NotFound();
        if (this.User == null) return this.StatusCode(403, "");

        this.IpAddress = this.HttpContext.Connection.RemoteIpAddress;

        this.AuthenticationAttempts = this.Database.AuthenticationAttempts.Include
                (a => a.GameToken)
            .Where(a => a.GameToken.UserId == this.User.UserId)
            .OrderByDescending(a => a.Timestamp)
            .ToList();

        return this.Page();
    }
}