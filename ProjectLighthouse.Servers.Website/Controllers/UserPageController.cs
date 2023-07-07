#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers;

[ApiController]
[Route("user/{id:int}")]
public class UserPageController : ControllerBase
{
    private readonly DatabaseContext database;

    public UserPageController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpGet("heart")]
    public async Task<IActionResult> HeartUser([FromRoute] int id)
    {
        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        UserEntity? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (heartedUser == null) return this.NotFound();

        await this.database.HeartUser(token.UserId, heartedUser);

        return this.Redirect("~/user/" + id);
    }

    [HttpGet("unheart")]
    public async Task<IActionResult> UnheartUser([FromRoute] int id)
    {
        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        UserEntity? heartedUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (heartedUser == null) return this.NotFound();

        await this.database.UnheartUser(token.UserId, heartedUser);

        return this.Redirect("~/user/" + id);
    }

    [HttpGet("block")]
    public async Task<IActionResult> BlockUser([FromRoute] int id)
    {
        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        UserEntity? blockedUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (blockedUser == null) return this.NotFound();

        await this.database.BlockUser(token.UserId, blockedUser);

        return this.Redirect("~/user/" + id);
    }

    [HttpGet("unblock")]
    public async Task<IActionResult> UnblockUser([FromRoute] int id)
    {
        WebTokenEntity? token = this.database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("~/login");

        UserEntity? blockedUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
        if (blockedUser == null) return this.NotFound();

        await this.database.UnblockUser(token.UserId, blockedUser);

        return this.Redirect("~/user/" + id);
    }
}