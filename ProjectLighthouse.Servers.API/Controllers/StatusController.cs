using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.API.Controllers;

[ApiController]
[Route("/api/v1")]
[Produces("application/json")]
public class StatusController : ControllerBase
{
    [HttpGet("status")]
    public IActionResult GetStatus() => this.Ok();
}