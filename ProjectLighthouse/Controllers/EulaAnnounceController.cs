using Microsoft.AspNetCore.Mvc;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/plain")]
    public class EulaAnnounceController : ControllerBase {
        [HttpGet("eula")]
        public IActionResult Eula() {
            return Ok("eula test");
        }

        [HttpGet("announce")]
        public IActionResult Announce() {
            return Ok("PROJECT LIGHTHOUSE IS A GO!\nalso ezoiar was here\nnow on ASP.NET!");
        }
    }
}