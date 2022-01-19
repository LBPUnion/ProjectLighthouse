#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.Website.ExternalAuth;

[ApiController]
[Route("/authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly Database database;

    public AuthenticationController(Database database)
    {
        this.database = database;
    }

    [HttpGet("approve/{id:int}")]
    public async Task<IActionResult> Approve(int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");

        AuthenticationAttempt? authAttempt = await this.database.AuthenticationAttempts.Include
                (a => a.GameToken)
            .FirstOrDefaultAsync(a => a.AuthenticationAttemptId == id);
        if (authAttempt == null) return this.NotFound();

        if (authAttempt.GameToken.UserId != user.UserId) return this.StatusCode(403, "");

        authAttempt.GameToken.Approved = true;
        this.database.AuthenticationAttempts.Remove(authAttempt);

        await this.database.SaveChangesAsync();

        return this.Redirect("~/authentication");
    }

    [HttpGet("deny/{id:int}")]
    public async Task<IActionResult> Deny(int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");

        AuthenticationAttempt? authAttempt = await this.database.AuthenticationAttempts.Include
                (a => a.GameToken)
            .FirstOrDefaultAsync(a => a.AuthenticationAttemptId == id);
        if (authAttempt == null) return this.NotFound();

        if (authAttempt.GameToken.UserId != user.UserId) return this.StatusCode(403, "");

        this.database.GameTokens.Remove(authAttempt.GameToken);
        this.database.AuthenticationAttempts.Remove(authAttempt);

        DeniedAuthenticationHelper.SetDeniedAt($"{authAttempt.IPAddress}|{user.Username}");

        await this.database.SaveChangesAsync();

        return this.Redirect("~/authentication");
    }

    [HttpGet("denyAll")]
    public async Task<IActionResult> DenyAll()
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");

        List<AuthenticationAttempt> authAttempts = await this.database.AuthenticationAttempts.Include
                (a => a.GameToken)
            .Where(a => a.GameToken.UserId == user.UserId)
            .ToListAsync();

        foreach (AuthenticationAttempt authAttempt in authAttempts)
        {
            this.database.GameTokens.Remove(authAttempt.GameToken);
            this.database.AuthenticationAttempts.Remove(authAttempt);

            DeniedAuthenticationHelper.SetDeniedAt($"{authAttempt.IPAddress}|{user.Username}");
        }

        await this.database.SaveChangesAsync();

        return this.Redirect("~/authentication");
    }
}