using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers;

[ApiController]
[Route("/")]
[Produces("application/json")]
public class StatusController : ControllerBase
{
    [HttpGet("status")]
    public IActionResult GetStatus() => this.Ok();
}