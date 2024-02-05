#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers.ExternalAuth;

[ApiController]
[Route("/authentication")]
public class AuthenticationController : ControllerBase
{
    private readonly DatabaseContext database;

    public AuthenticationController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpGet("unlink/{platform}")]
    public async Task<IActionResult> UnlinkPlatform(string platform)
    {
        UserEntity? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        Platform[] invalidTokens;

        if (platform == "psn")
        {
            user.LinkedPsnId = 0;
            invalidTokens = new[] { Platform.PS3, Platform.Vita, };
        }
        else
        {
            user.LinkedRpcnId = 0;
            invalidTokens = new[] { Platform.RPCS3, };
        }

        await this.database.GameTokens.RemoveWhere(t => t.UserId == user.UserId && invalidTokens.Contains(t.Platform));
        await this.database.SaveChangesAsync();

        return this.Redirect("~/authentication");
    }

    [HttpGet("approve/{id:int}")]
    public async Task<IActionResult> Approve(int id)
    {
        UserEntity? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");

        PlatformLinkAttemptEntity? linkAttempt = await this.database.PlatformLinkAttempts
            .FirstOrDefaultAsync(l => l.PlatformLinkAttemptId == id);
        if (linkAttempt == null) return this.NotFound();

        if (linkAttempt.UserId != user.UserId) return this.NotFound();

        if (linkAttempt.Platform == Platform.RPCS3)
        {
            user.LinkedRpcnId = linkAttempt.PlatformId;
        }
        else
        {
            user.LinkedPsnId = linkAttempt.PlatformId;
        }

        this.database.PlatformLinkAttempts.Remove(linkAttempt);

        await this.database.SaveChangesAsync();

        return this.Redirect("~/authentication");
    }

    [HttpGet("deny/{id:int}")]
    public async Task<IActionResult> Deny(int id)
    {
        UserEntity? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");

        PlatformLinkAttemptEntity? linkAttempt = await this.database.PlatformLinkAttempts
            .FirstOrDefaultAsync(l => l.PlatformLinkAttemptId == id);
        if (linkAttempt == null) return this.NotFound();

        if (linkAttempt.UserId != user.UserId) return this.NotFound();

        this.database.PlatformLinkAttempts.Remove(linkAttempt);

        await this.database.SaveChangesAsync();

        return this.Redirect("~/authentication");
    }
}