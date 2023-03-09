using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.API.Controllers;

public class StatusController : ApiEndpointController
{
    [HttpGet("status")]
    public IActionResult GetStatus() => this.Ok();
}