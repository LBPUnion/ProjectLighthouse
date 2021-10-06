using Microsoft.AspNetCore.Mvc;
using ProjectLighthouse.Serialization;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/network_settings.nws")]
    [Produces("text/xml")]
    public class NetworkSettingsController : ControllerBase {
        [HttpGet]
        public IActionResult Get() {
            return this.Ok(LbpSerializer.BlankElement("networkSettings"));
        }
    }
}