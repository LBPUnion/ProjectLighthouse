using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Controllers
{
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/enterLevel")]
//    [Produces("text/plain")]
    public class EnterLevelController : ControllerBase
    {
        [HttpGet("enterLevel/{id}")]
        public IActionResult EnterLevel(string id) => this.Ok();
    }
}