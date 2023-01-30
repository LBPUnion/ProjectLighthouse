using LBPUnion.ProjectLighthouse.Administration;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers.Moderator;

[ApiController]
[Route("moderation/case/{id:int}")]
public class ModerationCaseController : ControllerBase
{
    private readonly Database database;

    public ModerationCaseController(Database database)
    {
        this.database = database;
    }

    [HttpGet("dismiss")]
    public async Task<IActionResult> DismissCase([FromRoute] int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsModerator) return this.StatusCode(403, "");

        ModerationCase? @case = await this.database.Cases.FirstOrDefaultAsync(c => c.CaseId == id);
        if (@case == null) return this.NotFound();
        
        @case.DismissedAt = DateTime.Now;
        @case.DismisserId = user.UserId;
        @case.DismisserUsername = user.Username;
        
        @case.Processed = false;

        await this.database.SaveChangesAsync();
        
        return this.Ok();
    }
}