using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class StoreController : Controller
{
    [HttpGet("promotions")]
    public IActionResult Promotions() => this.Ok();
}