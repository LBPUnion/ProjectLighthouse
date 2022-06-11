#nullable enable
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers.ExternalAuth;

[ApiController]
[Route("/authentication")]
public class AutoApprovalController : ControllerBase
{
    private readonly Database database;

    public AutoApprovalController(Database database)
    {
        this.database = database;
    }

    [HttpGet("autoApprove/{id:int}")]
    public async Task<IActionResult> AutoApprove([FromRoute] int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");

        AuthenticationAttempt? authAttempt = await this.database.AuthenticationAttempts.Include
                (a => a.GameToken)
            .FirstOrDefaultAsync(a => a.AuthenticationAttemptId == id);

        if (authAttempt == null) return this.BadRequest();
        if (authAttempt.GameToken.UserId != user.UserId) return this.Redirect("/login");

        authAttempt.GameToken.Approved = true;
        user.ApprovedIPAddress = authAttempt.IPAddress;
        
        this.database.AuthenticationAttempts.Remove(authAttempt);

        await this.database.SaveChangesAsync();

        return this.Redirect("/authentication");
    }

    [HttpGet("revokeAutoApproval")]
    public async Task<IActionResult> RevokeAutoApproval()
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");

        user.ApprovedIPAddress = null;

        await this.database.SaveChangesAsync();

        return this.Redirect("/authentication");
    }
}