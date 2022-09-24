#nullable enable
using System.Net;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Match.Rooms;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Tickets;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

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
    public async Task<IActionResult> Login()
    {
        MemoryStream ms = new();
        await this.Request.Body.CopyToAsync(ms);
        byte[] loginData = ms.ToArray();

        NPTicket? npTicket;
        try
        {
            npTicket = NPTicket.CreateFromBytes(loginData);
        }
        catch
        {
            npTicket = null;
        }

        if (npTicket == null)
        {
            Logger.Warn("npTicket was null, rejecting login", LogArea.Login);
            return this.BadRequest();
        }

        IPAddress? remoteIpAddress = this.HttpContext.Connection.RemoteIpAddress;
        if (remoteIpAddress == null)
        {
            Logger.Warn("unable to determine ip, rejecting login", LogArea.Login);
            return this.StatusCode(403, ""); // 403 probably isnt the best status code for this, but whatever
        }

        string ipAddress = remoteIpAddress.ToString();

        await this.database.RemoveExpiredTokens();

        // Get an existing token from the IP & username
        GameToken? token = await this.database.GameTokens.Include(t => t.User)
            .FirstOrDefaultAsync(t => t.UserLocation == ipAddress && t.User.Username == npTicket.Username && !t.Used);

        if (token == null) // If we cant find an existing token, try to generate a new one
        {
            token = await this.database.AuthenticateUser(npTicket, ipAddress);
            if (token == null)
            {
                Logger.Warn($"Unable to find/generate a token for username {npTicket.Username}", LogArea.Login);
                return this.StatusCode(403, ""); // If not, then 403.
            }
        }

        User? user = await this.database.UserFromGameToken(token, true);

        if (user == null || user.IsBanned)
        {
            Logger.Error($"Unable to find user {npTicket.Username} from token", LogArea.Login);
            return this.StatusCode(403, "");
        }

        if (ServerConfiguration.Instance.Mail.MailEnabled && (user.EmailAddress == null || !user.EmailAddressVerified))
        {
            Logger.Error($"Email address unverified for user {user.Username}", LogArea.Login);
            return this.StatusCode(403, "");
        }

        if (ServerConfiguration.Instance.Authentication.UseExternalAuth)
        {
            if (user.ApprovedIPAddress == ipAddress)
            {
                token.Approved = true;
            }
            else
            {
                AuthenticationAttempt authAttempt = new()
                {
                    GameToken = token,
                    GameTokenId = token.TokenId,
                    Timestamp = TimeHelper.Timestamp,
                    IPAddress = ipAddress,
                    Platform = npTicket.Platform,
                };

                this.database.AuthenticationAttempts.Add(authAttempt);
            }
        }
        else
        {
            token.Approved = true;
        }

        await this.database.SaveChangesAsync();

        if (!token.Approved)
        {
            Logger.Warn($"Token unapproved for user {user.Username}, rejecting login", LogArea.Login);
            return this.StatusCode(403, "");
        }

        Logger.Success($"Successfully logged in user {user.Username} as {token.GameVersion} client", LogArea.Login);
        // After this point we are now considering this session as logged in.

        // We just logged in with the token. Mark it as used so someone else doesnt try to use it,
        // and so we don't pick the same token up when logging in later.
        token.Used = true;

        user.LastLogin = TimeHelper.TimestampMillis;

        await this.database.SaveChangesAsync();

        // Create a new room on LBP2/3/Vita
        if (token.GameVersion != GameVersion.LittleBigPlanet1) RoomHelper.CreateRoom(user.UserId, token.GameVersion, token.Platform);

        return this.Ok
        (
            new LoginResult
            {
                AuthTicket = "MM_AUTH=" + token.UserToken,
                ServerBrand = VersionHelper.EnvVer,
                TitleStorageUrl = ServerConfiguration.Instance.GameApiExternalUrl,
            }.Serialize()
        );
    }
}