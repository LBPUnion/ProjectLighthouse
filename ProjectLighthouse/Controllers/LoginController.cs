#nullable enable
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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

            IPAddress? remoteIpAddress = this.HttpContext.Connection.RemoteIpAddress;
            if (remoteIpAddress == null) return this.StatusCode(403, ""); // 403 probably isnt the best status code for this, but whatever

            string ipAddress = remoteIpAddress.ToString();

            // Get an existing token from the IP & username
            GameToken? token = await this.database.GameTokens.Include
                    (t => t.User)
                .FirstOrDefaultAsync(t => t.UserLocation == ipAddress && t.User.Username == loginData.Username && !t.Used);

            if (token == null) // If we cant find an existing token, try to generate a new one
            {
                token = await this.database.AuthenticateUser(loginData, ipAddress, titleId);
                if (token == null) return this.StatusCode(403, ""); // If not, then 403.
            }

            User? user = await this.database.UserFromGameToken(token, true);
            if (user == null) return this.StatusCode(403, "");

            if (ServerSettings.Instance.UseExternalAuth)
            {
                if (ServerSettings.Instance.BlockDeniedUsers)
                {
                    string ipAddressAndName = $"{token.UserLocation}|{user.Username}";
                    if (DeniedAuthenticationHelper.RecentlyDenied(ipAddressAndName) || DeniedAuthenticationHelper.GetAttempts(ipAddressAndName) > 3)
                    {
                        this.database.AuthenticationAttempts.RemoveRange
                            (this.database.AuthenticationAttempts.Include(a => a.GameToken).Where(a => a.GameToken.UserId == user.UserId));

                        DeniedAuthenticationHelper.AddAttempt(ipAddressAndName);

                        await this.database.SaveChangesAsync();
                        return this.StatusCode(403, "");
                    }
                }

                if (this.database.UserApprovedIpAddresses.Where(a => a.UserId == user.UserId).Select(a => a.IpAddress).Contains(ipAddress))
                {
                    token.Approved = true;
                }
                else
                {
                    AuthenticationAttempt authAttempt = new()
                    {
                        GameToken = token,
                        GameTokenId = token.TokenId,
                        Timestamp = TimestampHelper.Timestamp,
                        IPAddress = ipAddress,
                        Platform = token.GameVersion == GameVersion.LittleBigPlanetVita ? Platform.Vita : Platform.PS3, // TODO: properly identify RPCS3
                    };

                    this.database.AuthenticationAttempts.Add(authAttempt);
                }
            }
            else
            {
                token.Approved = true;
            }

            await this.database.SaveChangesAsync();

            if (!token.Approved) return this.StatusCode(403, "");

            Logger.Log($"Successfully logged in user {user.Username} as {token.GameVersion} client ({titleId})", LoggerLevelLogin.Instance);
            // After this point we are now considering this session as logged in.

            // We just logged in with the token. Mark it as used so someone else doesnt try to use it,
            // and so we don't pick the same token up when logging in later.
            token.Used = true;

            await this.database.SaveChangesAsync();

            // Create a new room on LBP2/3/Vita
            if (token.GameVersion != GameVersion.LittleBigPlanet1) RoomHelper.CreateRoom(user);

            return this.Ok
            (
                new LoginResult
                {
                    AuthTicket = "MM_AUTH=" + token.UserToken,
                    LbpEnvVer = ServerStatics.ServerName,
                }.Serialize()
            );
        }
    }
}