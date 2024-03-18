#nullable enable
using System.Net;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Tickets;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Login;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/login")]
[Produces("text/xml")]
public class LoginController : ControllerBase
{
    private readonly DatabaseContext database;

    public LoginController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpPost]
    public async Task<IActionResult> Login()
    {
        byte[] loginData = await this.Request.BodyReader.ReadAllAsync();

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
            return this.Forbid();
        }

        await this.database.RemoveExpiredTokens();

        UserEntity? user;

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

        // If this user id hasn't been linked to any accounts
        if (user == null)
        {
            // Check if there is an account with that username already
            UserEntity? targetUsername = await this.database.Users.FirstOrDefaultAsync(u => u.Username == npTicket.Username);
            if (targetUsername != null)
            {
                ulong targetPlatform = npTicket.Platform == Platform.RPCS3
                    ? targetUsername.LinkedRpcnId
                    : targetUsername.LinkedPsnId;

                // only make a link request if the user doesn't already have an account linked for that platform
                if (targetPlatform != 0)
                {
                    Logger.Warn($"New user tried to login but their name is already taken, username={username}", LogArea.Login);
                    return this.Forbid();
                }

                // if there is already a pending link request don't create another
                bool linkAttemptExists = await this.database.PlatformLinkAttempts.AnyAsync(p =>
                    p.Platform == npTicket.Platform &&
                    p.PlatformId == npTicket.UserId &&
                    p.UserId == targetUsername.UserId);

                if (linkAttemptExists) return this.Forbid();

                PlatformLinkAttemptEntity linkAttempt = new()
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
                return this.Forbid();
            }

            if (!ServerConfiguration.Instance.Authentication.AutomaticAccountCreation)
            {
                Logger.Warn($"Unknown user tried to connect username={username}", LogArea.Login);
                return this.Forbid();
            }
            // create account for user if they don't exist
            user = await this.database.CreateUser(username, "$");
            user.Password = null;
            user.LinkedRpcnId = npTicket.Platform == Platform.RPCS3 ? npTicket.UserId : 0;
            user.LinkedPsnId = npTicket.Platform != Platform.RPCS3 ? npTicket.UserId : 0;
            await this.database.SaveChangesAsync();

            if (DiscordConfiguration.Instance.DiscordIntegrationEnabled)
            {
                string registrationAnnouncementMsg = DiscordConfiguration.Instance.RegistrationAnnouncement
                    .Replace("%user", username)
                    .Replace("%id", user.UserId.ToString())
                    .Replace("%instance", ServerConfiguration.Instance.Customization.ServerName)
                    .Replace("%platform", npTicket.Platform.ToString())
                    .Replace(@"\n", "\n");
                await WebhookHelper.SendWebhook(title: "A new user has registered!",
                    description: registrationAnnouncementMsg,
                    dest: WebhookHelper.WebhookDestination.Registration);
            }

            Logger.Success($"Created new user for {username}, platform={npTicket.Platform}", LogArea.Login);
        }
        // automatically change username if it doesn't match
        else if (user.Username != npTicket.Username)
        {
            bool usernameExists = await this.database.Users.AnyAsync(u => u.Username == npTicket.Username);
            if (usernameExists)
            {
                Logger.Warn($"{npTicket.Platform} user changed their name to a name that is already taken," +
                            $" oldName='{user.Username}', newName='{npTicket.Username}'", LogArea.Login);
                return this.Forbid();
            }
            Logger.Info($"User's username has changed, old='{user.Username}', new='{npTicket.Username}', platform={npTicket.Platform}", LogArea.Login);
            user.Username = username;
            await this.database.PlatformLinkAttempts.RemoveWhere(p => p.UserId == user.UserId);
            // unlink other platforms because the names no longer match
            if (npTicket.Platform == Platform.RPCS3)
                user.LinkedPsnId = 0;
            else
                user.LinkedRpcnId = 0;

            await this.database.SaveChangesAsync();
        }

        GameTokenEntity? token = await this.database.GameTokens.Include(t => t.User)
            .FirstOrDefaultAsync(t => t.User.Username == npTicket.Username && t.TicketHash == npTicket.TicketHash);

        if (token != null)
        {
            Logger.Warn($"Rejecting duplicate ticket from {username}", LogArea.Login);
            return this.Forbid();
        }

        token = await this.database.AuthenticateUser(user, npTicket, ipAddress);
        if (token == null)
        {
            Logger.Warn($"Unable to find/generate a token for username {npTicket.Username}", LogArea.Login);
            return this.Forbid();
        }

        if (user.IsBanned)
        {
            Logger.Error($"User {npTicket.Username} tried to login but is banned", LogArea.Login);
            return this.Forbid();
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
            }
        );
    }
}