using Microsoft.AspNetCore.Mvc;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/enterLevel")]
//    [Produces("text/plain")]
    public class EnterLevelController : ControllerBase {
        [HttpGet("enterLevel/{id}")]
        public IActionResult EnterLevel(string id) {
            return this.Ok();
        }
    }
}