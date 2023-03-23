using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers;

[ApiController]
[Route("/")]
[Produces("application/json")]
public class StatusController : ControllerBase
{
    [AcceptVerbs("GET", "HEAD", Route = "status")]
    public IActionResult GetStatus() => this.Ok();
}