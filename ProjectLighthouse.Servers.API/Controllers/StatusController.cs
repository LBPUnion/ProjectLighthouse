using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.API.Controllers;

public class StatusController : ApiEndpointController
{
    [AcceptVerbs("GET", "HEAD", Route = "status")]
    public IActionResult GetStatus() => this.Ok();
}