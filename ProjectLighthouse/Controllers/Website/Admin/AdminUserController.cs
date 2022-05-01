#nullable enable
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.Website.Admin;

[ApiController]
[Route("admin/user/{id:int}")]
public class AdminUserController : ControllerBase
{
    private readonly Database database;

    public AdminUserController(Database database)
    {
        this.database = database;
    }

    [HttpGet("unban")]
    public async Task<IActionResult> UnbanUser([FromRoute] int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.NotFound();

        User? targetedUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);

        if (targetedUser == null) return this.NotFound();

        targetedUser.Banned = false;
        targetedUser.BannedReason = null;

        await this.database.SaveChangesAsync();
        return this.Redirect($"/user/{targetedUser.UserId}");
    }
}