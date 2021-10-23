using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Controllers {
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
            User user = await this.database.UserFromRequest(this.Request);
            return user == null ? this.StatusCode(403, "") : this.Ok($"You are logged in as user {user.Username} (id {user.UserId})");
        }

        [HttpGet("announce")]
        public IActionResult Announce() {
            return this.Ok("");
        }

        [HttpGet("notification")]
        public IActionResult Notification() {
            return this.Ok();
        }

        [HttpPost("filter")]
        public IActionResult Filter() {
            return this.Ok();
        }
    }
}
