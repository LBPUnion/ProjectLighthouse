#nullable enable
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.Website.ExternalAuth;

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

        UserApprovedIpAddress approvedIpAddress = new()
        {
            UserId = user.UserId,
            User = user,
            IpAddress = authAttempt.IPAddress,
        };

        this.database.UserApprovedIpAddresses.Add(approvedIpAddress);
        this.database.AuthenticationAttempts.Remove(authAttempt);

        await this.database.SaveChangesAsync();

        return this.Redirect("/authentication");
    }

    [HttpGet("revokeAutoApproval/{id:int}")]
    public async Task<IActionResult> RevokeAutoApproval([FromRoute] int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");

        UserApprovedIpAddress? approvedIpAddress = await this.database.UserApprovedIpAddresses.FirstOrDefaultAsync(a => a.UserApprovedIpAddressId == id);
        if (approvedIpAddress == null) return this.BadRequest();
        if (approvedIpAddress.UserId != user.UserId) return this.Redirect("/login");

        this.database.UserApprovedIpAddresses.Remove(approvedIpAddress);

        await this.database.SaveChangesAsync();

        return this.Redirect("/authentication/autoApprovals");
    }
}