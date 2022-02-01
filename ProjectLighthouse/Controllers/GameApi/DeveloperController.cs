using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Controllers.GameApi;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
public class DeveloperController : Controller
{
    [HttpGet("/developer_videos")]
    public IActionResult DeveloperVideos() => this.Ok();
}