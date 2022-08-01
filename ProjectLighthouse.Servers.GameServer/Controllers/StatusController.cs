using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

[ApiController]
[Route("/LITTLEBIGPLANETPS3_XML")]
[Produces("application/json")]
public class StatusController : ControllerBase
{
    [HttpGet("status")]
    public IActionResult GetStatus() => this.Ok();
}