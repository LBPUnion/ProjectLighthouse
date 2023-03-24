using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.API.Controllers;

[ApiController]
[Route("/api/v1")]
[Produces("application/json")]
public class StatusController : ControllerBase
{
    [AcceptVerbs("GET", "HEAD", Route = "status")]
    public IActionResult GetStatus() => this.Ok();
}