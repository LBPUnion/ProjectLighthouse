using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ProjectLighthouse.Types;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/login")]
    [Produces("text/xml")]
    public class LoginController : ControllerBase {
        [HttpGet]
        [HttpPost]
        public IActionResult Post() {
            if(!this.Request.Query.TryGetValue("titleID", out StringValues _)) {
                this.BadRequest();
            }

//            string titleId = titleValues[0];

            return this.Ok(new LoginResult {
                AuthTicket = "d2c6bbec59162a1e786ed24ad95f2b73",
                LbpEnvVer = "ProjectLighthouse"
            }.Serialize());
        }
    }
}