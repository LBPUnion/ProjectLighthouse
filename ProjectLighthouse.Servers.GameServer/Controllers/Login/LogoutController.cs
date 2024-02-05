using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Login;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/goodbye")]
[Produces("text/xml")]
public class LogoutController : ControllerBase
{

    private readonly DatabaseContext database;

    public LogoutController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpPost]
    public async Task<IActionResult> OnPost()
    {
        GameTokenEntity token = this.GetToken();

        UserEntity? user = await this.database.UserFromGameToken(token);
        if (user == null) return this.Forbid();

        user.LastLogout = TimeHelper.TimestampMillis;

        await this.database.GameTokens.RemoveWhere(t => t.TokenId == token.TokenId);
        await this.database.LastContacts.RemoveWhere(c => c.UserId == token.UserId);

        return this.Ok();
    }

    
}