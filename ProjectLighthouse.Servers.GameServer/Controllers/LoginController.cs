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

        string? username = npTicket.Username;

        if (username == null)
        {
            Logger.Warn("Unable to determine username, rejecting login", LogArea.Login);
            return this.StatusCode(403, "");
        }

        await this.database.RemoveExpiredTokens();

        User? user;

        switch (npTicket.Platform)
        {
            case Platform.RPCS3:
                user = await this.database.Users.FirstOrDefaultAsync(u => u.LinkedRpcnId == npTicket.UserId); 
                break;
            case Platform.PS3:
            case Platform.Vita:
                user = await this.database.Users.FirstOrDefaultAsync(u => u.LinkedPsnId == npTicket.UserId);
                break;
            case Platform.PSP:
            case Platform.UnitTest:
            case Platform.Unknown:
            default:
                throw new ArgumentOutOfRangeException();
        }

        // create new user and link id
        if (user == null)
        {
            User? targetUsername = await this.database.Users.FirstOrDefaultAsync(u => u.Username == npTicket.Username);
            if (targetUsername != null)
            {
                // WebToken? webToken = await this.database.WebTokens.Where(t => t.UserId == targetUsername.UserId)
                    // .Where(t => t.UserLocation == ipAddress)
                    // .FirstOrDefaultAsync();
                    ulong targetPlatform = npTicket.Platform == Platform.RPCS3
                        ? targetUsername.LinkedRpcnId
                        : targetUsername.LinkedPsnId;
                // try and link platform with account
                if (targetPlatform == 0)
                {
                    if (await this.database.PlatformLinkAttempts.AnyAsync(p =>
                            p.Platform == npTicket.Platform &&
                            p.PlatformId == npTicket.UserId &&
                            p.UserId == targetUsername.UserId))
                    {
                        return this.StatusCode(403, "");
                    }
                    PlatformLinkAttempt linkAttempt = new()
                    {
                        Platform = npTicket.Platform,
                        UserId = targetUsername.UserId,
                        IPAddress = ipAddress,
                        Timestamp = TimeHelper.TimestampMillis,
                        PlatformId = npTicket.UserId,
                    };
                    this.database.PlatformLinkAttempts.Add(linkAttempt);
                    await this.database.SaveChangesAsync();
                    Logger.Success($"Linked {npTicket.Platform} account to user '{targetUsername.Username}'", LogArea.Login);
                    return this.StatusCode(403, "");
                }
                Logger.Warn($"New user tried to login but their name is already taken, username={username}", LogArea.Login);
                return this.StatusCode(403, "");
            }

            if (!ServerConfiguration.Instance.Authentication.AutomaticAccountCreation)
            {
                Logger.Warn($"Unknown user tried to connect username={username}", LogArea.Login);
                return this.StatusCode(403, "");
            }
            // create account for user if they don't exist
            user = await this.database.CreateUser(username, "$");
            user.Password = null;
            user.LinkedRpcnId = npTicket.Platform == Platform.RPCS3 ? npTicket.UserId : 0;
            user.LinkedPsnId = npTicket.Platform != Platform.RPCS3 ? npTicket.UserId : 0;
            await this.database.SaveChangesAsync();
                
            Logger.Success($"Created new user for {username}, platform={npTicket.Platform}", LogArea.Login);
        }
        // if a user changes their username invalidate their other linked accounts
        // because they will no longer have the same name
        // TODO a psn user can probably steal an rpcs3 username, what the fuck do we do
        else if (user.Username != npTicket.Username)
        {
            Logger.Info($"User's username has changed, old='{user.Username}', new='{npTicket.Username}', platform={npTicket.Platform}", LogArea.Login);
            user.Username = username;
            if (npTicket.Platform == Platform.RPCS3)
            {
                user.LinkedPsnId = 0;
            }
            else
            {
                user.LinkedRpcnId = 0;
            }
        }

        // Get an existing token from the IP & username
        GameToken? token = await this.database.GameTokens.Include(t => t.User)
            .FirstOrDefaultAsync(t => t.UserLocation == ipAddress && t.User.Username == npTicket.Username && t.TicketHash == npTicket.TicketSerial);

        if (token != null)
        {
            Logger.Warn($"Rejecting duplicate ticket from {username}", LogArea.Login);
            return this.StatusCode(403, "");
        }

        token = await this.database.AuthenticateUser(user, npTicket, ipAddress);
        if (token == null)
        {
            Logger.Warn($"Unable to find/generate a token for username {npTicket.Username}", LogArea.Login);
            return this.StatusCode(403, ""); // If not, then 403.
        }


        // The GameToken LINQ statement above is case insensitive so we check that they are equal here
        if (token.User.Username != npTicket.Username)
        {
            Logger.Warn($"Username case does not match for user {npTicket.Username}, expected={token.User.Username}", LogArea.Login);
            return this.StatusCode(403, "");
        }

        if (user.IsBanned)
        {
            Logger.Error($"User {npTicket.Username} tried to login but is banned", LogArea.Login);
            return this.StatusCode(403, "");
        }

        await this.database.SaveChangesAsync();

        Logger.Success($"Successfully logged in user {user.Username} as {token.GameVersion} client", LogArea.Login);
        // After this point we are now considering this session as logged in.

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