using System.IO;
using System.Threading.Tasks;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Controllers
{
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/plain")]
    public class MessageController : ControllerBase
    {
        private readonly Database database;

        public MessageController(Database database)
        {
            this.database = database;
        }

        [HttpGet("eula")]
        public async Task<IActionResult> Eula()
        {
            User user = await this.database.UserFromRequest(this.Request);
            return user == null
                ? this.StatusCode(403, "")
                : this.Ok
                (
                    $"You are now logged in as user {user.Username} (id {user.UserId}).\n" +
                    // ReSharper disable once UnreachableCode
                    (EulaHelper.ShowPrivateInstanceNotice ? "\n" + EulaHelper.PrivateInstanceNotice : "") +
                    "\n" +
                    $"{EulaHelper.License}\n"
                );
        }

        [HttpGet("announce")]
        public IActionResult Announce() => this.Ok("");

        [HttpGet("notification")]
        public IActionResult Notification() => this.Ok();
        /// <summary>
        ///     Filters chat messages sent by a user.
        ///     The reponse sent is the text that will appear in-game.
        /// </summary>
        [HttpPost("filter")]
        public async Task<IActionResult> Filter()
        {
            User user = await this.database.UserFromRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            string loggedText = await new StreamReader(this.Request.Body).ReadToEndAsync();

            Logger.Log($"{user.Username}: {loggedText}", LoggerLevelFilter.Instance);
            return this.Ok(loggedText);
        }
    }
}