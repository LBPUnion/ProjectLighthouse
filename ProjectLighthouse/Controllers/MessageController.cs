using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectLighthouse.Types;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/plain")]
    public class MessageController : ControllerBase {
        private readonly Database database;
        public MessageController(Database database) {
            this.database = database;
        }

        [HttpGet("eula")]
        public async Task<IActionResult> Eula() {
            User user = await this.database.UserFromRequest(Request);
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
