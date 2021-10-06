using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectLighthouse.Serialization;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/slots")]
    [Produces("text/xml")]
    public class SlotsController : ControllerBase {
        [HttpGet("by")]
        public IActionResult SlotsBy() {
            return this.Ok(LbpSerializer.BlankElement("slots"));
        }
    }
}