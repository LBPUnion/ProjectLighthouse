using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

// Prototyping

[Route("debug")]
public class ActivityController
{
    private readonly Database database;

    public ActivityController(Database _database)
    {
        database = _database;
    }

    [HttpGet("actget")]
    public IActionResult DebugGetDBActUnauthorized()
    {
        JsonResult result = new JsonResult(database.Users.Include(u => u.Location).Include(u => u.PlayerEvents).FirstOrDefault(u => u.UserId == 2));
        return result;
    }
}