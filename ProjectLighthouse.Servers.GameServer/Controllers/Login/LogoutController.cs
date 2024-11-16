using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Types;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Login;

public class LogoutController : GameController
{
    private readonly DatabaseContext database;

    public LogoutController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpPost("goodbye")]
    public async Task<IActionResult> OnLogout()
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