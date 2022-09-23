using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/goodbye")]
[Produces("text/xml")]
public class LogoutController : ControllerBase
{

    private readonly Database database;

    public LogoutController(Database database)
    {
        this.database = database;
    }

    [HttpPost]
    public async Task<IActionResult> OnPost()
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        User? user = await this.database.Users.Where(u => u.UserId == token.UserId).FirstOrDefaultAsync();
        if (user == null) return this.StatusCode(403, "");

        user.LastLogout = TimeHelper.TimestampMillis;

        this.database.GameTokens.RemoveWhere(t => t.TokenId == token.TokenId);
        this.database.LastContacts.RemoveWhere(c => c.UserId == token.UserId);
        await this.database.SaveChangesAsync();

        return this.Ok();
    }

    
}