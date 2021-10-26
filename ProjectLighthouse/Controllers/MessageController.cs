using System.IO;
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
            return user == null ? this.StatusCode(403, "") : 
                this.Ok($"You are now logged in as user {user.Username} (id {user.UserId}).\n" +
                        "This is a private testing instance. Please do not make anything public for now, and keep in mind security isn't as tight as a full release would.");
        }

        [HttpGet("announce")]
        public IActionResult Announce() {
            return this.Ok("");
        }

        [HttpGet("notification")]
        public IActionResult Notification() {
            return this.Ok();
        }
        /// <summary>
        /// Filters chat messages sent by a user.
        /// </summary>
        [HttpPost("filter")]
        public async Task<IActionResult> Filter() {
            return this.Ok(await new StreamReader(this.Request.Body).ReadToEndAsync());
        }
    }
}