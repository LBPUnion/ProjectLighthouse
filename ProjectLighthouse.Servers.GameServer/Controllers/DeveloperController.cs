using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class DeveloperController : Controller
{
    [HttpGet("developer_videos")]
    public IActionResult DeveloperVideos() => this.Ok("<videos></videos>");
}