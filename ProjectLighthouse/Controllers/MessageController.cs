using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectLighthouse.Types;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/plain")]
    public class MessageController : ControllerBase {
        [HttpGet("eula")]
        public async Task<IActionResult> Eula() {
            User user = await new Database().UserFromRequest(Request);
            return user == null ? this.StatusCode(403, "") : this.Ok($"You are logged in as user {user.Username} (id {user.UserId})");
        }

        [HttpGet("announce")]
        public IActionResult Announce() {
            return Ok("");
        }

        [HttpGet("notification")]
        public IActionResult Notification() {
            return this.Ok();
        }
    }
}
