using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

[ApiController]
[Route("/LITTLEBIGPLANETPS3_XML")]
[Produces("application/json")]
public class StatusController : ControllerBase
{
    [AcceptVerbs("GET", "HEAD", Route = "status")]
    public IActionResult GetStatus() => this.Ok();
}