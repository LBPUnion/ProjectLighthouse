using System;
using System.Collections.Generic;
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
            return this.Ok(new LoginResult {
                AuthTicket = "d2c6bbec59162a1e786ed24ad95f2b73",
                LbpEnvVer = "rLBP_Cepheus"
            }.Serialize());
        }
    }
}