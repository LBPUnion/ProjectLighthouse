#nullable enable
using System.IO;
using System.Net;
using System.Threading.Tasks;
using Kettu;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Controllers
{
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/login")]
    [Produces("text/xml")]
    public class LoginController : ControllerBase
    {
        private readonly Database database;

        public LoginController(Database database)
        {
            this.database = database;
        }

        [HttpPost]
        public async Task<IActionResult> Login([FromQuery] string? titleId)
        {
            titleId ??= "";

            string body = await new StreamReader(this.Request.Body).ReadToEndAsync();

            LoginData? loginData;
            try
            {
                loginData = LoginData.CreateFromString(body);
            }
            catch
            {
                loginData = null;
            }
            if (loginData == null) return this.BadRequest();

            IPAddress? ipAddress = this.HttpContext.Connection.RemoteIpAddress;
            if (ipAddress == null) return this.StatusCode(403, ""); // 403 probably isnt the best status code for this, but whatever

            string userLocation = ipAddress.ToString();

            Token? token = await this.database.AuthenticateUser(loginData, userLocation, titleId);
            if (token == null) return this.StatusCode(403, "");

            Logger.Log
            (
                $"Successfully logged in user {(await this.database.UserFromToken(token))!.Username} as {token.GameVersion} client ({titleId})",
                LoggerLevelLogin.Instance
            );

            return this.Ok
            (
                new LoginResult
                {
                    AuthTicket = "MM_AUTH=" + token.UserToken,
                    LbpEnvVer = ServerSettings.ServerName,
                }.Serialize()
            );
        }
    }
}