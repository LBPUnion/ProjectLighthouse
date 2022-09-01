using LBPUnion.ProjectLighthouse.PlayerData;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class DeveloperController : Controller
{

    private readonly Database database;

    public DeveloperController(Database database)
    {
        this.database = database;
    }

    [HttpGet("/developer_videos")]
    public async Task<IActionResult> DeveloperVideos()
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);

        if (token == null) return this.StatusCode(403, "");

        return this.Ok("<videos></videos>");
    }
}