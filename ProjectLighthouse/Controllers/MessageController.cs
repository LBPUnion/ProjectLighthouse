using System.IO;
using System.Threading.Tasks;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Settings;
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
        public IActionResult Eula() => this.Ok(ServerSettings.Instance.EulaText + "\n" + $"{EulaHelper.License}\n");

        [HttpGet("announce")]
        public async Task<IActionResult> Announce()
        {
            User user = await this.database.UserFromGameRequest(this.Request, true);
            if (user == null) return this.StatusCode(403, "");

            if (ServerSettings.Instance.UseExternalAuth)
                return this.Ok
                (
                    "Please stay on this screen.\n" +
                    $"Before continuing, you must approve this session at {ServerSettings.Instance.ExternalUrl}.\n" +
                    "Please keep in mind that if the session is denied you may have to wait up to 5-10 minutes to try logging in again.\n" +
                    "Once approved, you may press X and continue.\n\n" +
                    ServerSettings.Instance.EulaText
                );

            return this.Ok($"You are now logged in as {user.Username} (id: {user.UserId}).\n\n" + ServerSettings.Instance.EulaText);
        }

        [HttpGet("notification")]
        public IActionResult Notification() => this.Ok();
        /// <summary>
        ///     Filters chat messages sent by a user.
        ///     The reponse sent is the text that will appear in-game.
        /// </summary>
        [HttpPost("filter")]
        public async Task<IActionResult> Filter()
        {
            User user = await this.database.UserFromGameRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            string loggedText = await new StreamReader(this.Request.Body).ReadToEndAsync();

            Logger.Log($"{user.Username}: {loggedText}", LoggerLevelFilter.Instance);
            return this.Ok(loggedText);
        }
    }
}