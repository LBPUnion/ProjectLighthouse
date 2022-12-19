#nullable enable
using System.Net;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
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
            return this.BadRequest();
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
            case Platform.UnitTest:
                user = await this.database.Users.FirstOrDefaultAsync(u => u.LinkedPsnId == npTicket.UserId);
                break;
            case Platform.PSP:
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
                ulong targetPlatform = npTicket.Platform == Platform.RPCS3
                        ? targetUsername.LinkedRpcnId
                        : targetUsername.LinkedPsnId;
                // only make a link request if the user doesn't already have an account linked for that platform
                if (targetPlatform == 0)
                {
                    // if there is already a pending link request don't create another
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
                    Logger.Success($"User '{npTicket.Username}' tried to login but platform isn't linked, platform={npTicket.Platform}", LogArea.Login);
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
        else if (user.Username != npTicket.Username)
        {
            bool usernameExists = await this.database.Users.AnyAsync(u => u.Username == npTicket.Username);
            if (usernameExists)
            {
                Logger.Warn($"{npTicket.Platform} user changed their name to a name that is already taken, oldName='{user.Username}', newName='{npTicket.Username}'", LogArea.Login);
                return this.StatusCode(403, "");
            }
            Logger.Info($"User's username has changed, old='{user.Username}', new='{npTicket.Username}', platform={npTicket.Platform}", LogArea.Login);
            user.Username = username;
            this.database.PlatformLinkAttempts.RemoveWhere(p => p.UserId == user.UserId);
            // unlink other platforms because the names no longer match
            if (npTicket.Platform == Platform.RPCS3)
            {
                user.LinkedPsnId = 0;
            }
            else
            {
                user.LinkedRpcnId = 0;
            }
        }

        GameToken? token = await this.database.GameTokens.Include(t => t.User)
            .FirstOrDefaultAsync(t => t.UserLocation == ipAddress && t.User.Username == npTicket.Username && t.TicketHash == npTicket.TicketHash);

        if (token != null)
        {
            Logger.Warn($"Rejecting duplicate ticket from {username}", LogArea.Login);
            return this.StatusCode(403, "");
        }

        token = await this.database.AuthenticateUser(user, npTicket, ipAddress);
        if (token == null)
        {
            Logger.Warn($"Unable to find/generate a token for username {npTicket.Username}", LogArea.Login);
            return this.StatusCode(403, "");
        }

        if (user.IsBanned)
        {
            Logger.Error($"User {npTicket.Username} tried to login but is banned", LogArea.Login);
            return this.StatusCode(403, "");
        }

        Logger.Success($"Successfully logged in user {user.Username} as {token.GameVersion} client", LogArea.Login);

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