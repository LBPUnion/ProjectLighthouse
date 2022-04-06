#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.GameApi;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class UserController : ControllerBase
{
    private readonly Database database;

    public UserController(Database database)
    {
        this.database = database;
    }

    private async Task<string?> getSerializedUser(string username, GameVersion gameVersion = GameVersion.LittleBigPlanet1)
    {
        User? user = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.Username == username);
        return user?.Serialize(gameVersion);
    }

    private async Task<string?> getSerializedUserPicture(string username)
    {
        // use an anonymous type to only fetch certain columns
        var partialUser = await this.database.Users.Where(u => u.Username == username)
            .Select(u => new
            {
                u.Username,
                u.IconHash,
            }).FirstOrDefaultAsync();
        if (partialUser == null) return null;
        string user = LbpSerializer.TaggedStringElement("npHandle", partialUser.Username, "icon", partialUser.IconHash);
        return LbpSerializer.TaggedStringElement("user", user, "type", "user");
    }

    [HttpGet("user/{username}")]
    public async Task<IActionResult> GetUser(string username)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        string? user = await this.getSerializedUser(username, token.GameVersion);
        if (user == null) return this.NotFound();

        return this.Ok(user);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUserAlt([FromQuery] string[] u)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        List<string?> serializedUsers = new();
        foreach (string userId in u) serializedUsers.Add(await this.getSerializedUserPicture(userId));

        string serialized = serializedUsers.Aggregate(string.Empty, (current, user) => user == null ? current : current + user);

        return this.Ok(LbpSerializer.StringElement("users", serialized));
    }

    [HttpPost("updateUser")]
    public async Task<IActionResult> UpdateUser()
    {
        (User, GameToken)? userAndToken = await this.database.UserAndGameTokenFromRequest(this.Request);

        if (userAndToken == null) return this.StatusCode(403, "");

        // ReSharper disable once PossibleInvalidOperationException
        User user = userAndToken.Value.Item1;
        GameToken gameToken = userAndToken.Value.Item2;


        this.Request.Body.Position = 0;
        string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();
        // xml hack so we can use one class to deserialize different root names
        string rootElement = bodyString.Contains("updateUser") ? "updateUser" : "user";
        XmlSerializer serializer = new(typeof(UserUpdate), new XmlRootAttribute(rootElement));
        UserUpdate? update = (UserUpdate?) serializer.Deserialize(new StringReader(bodyString));

        if (update == null) return this.BadRequest();

        SanitizationHelper.SanitizeStringsInClass(update);

        if (update.Biography != null)
        {
            if (update.Biography.Length > 100) return this.BadRequest();

            user.Biography = update.Biography;
        }

        foreach (string? resource in new[] {update.IconHash, update.YayHash, update.MehHash, update.BooHash, update.PlanetHash,})
        {
            if (resource != null && !FileHelper.ResourceExists(resource)) return this.BadRequest();
        }
        
        if (update.IconHash != null) user.IconHash = update.IconHash;

        if (update.YayHash != null) user.YayHash = update.YayHash;

        if (update.MehHash != null) user.MehHash = update.MehHash;

        if (update.BooHash != null) user.BooHash = update.BooHash;

        if (update.PlanetHash != null)
        {
            switch (gameToken.GameVersion)
            {
                case GameVersion.LittleBigPlanet2: // LBP2 planets will apply to LBP3
                {
                    user.PlanetHashLBP2 = update.PlanetHash;
                    user.PlanetHashLBP3 = update.PlanetHash;
                    break;
                }
                case GameVersion.LittleBigPlanet3: // LBP3 and vita can only apply to their own games, only set 1 here
                {
                    user.PlanetHashLBP3 = update.PlanetHash;
                    break;
                }
                case GameVersion.LittleBigPlanetVita:
                {
                    user.PlanetHashLBPVita = update.PlanetHash;
                    break;
                }
                case GameVersion.LittleBigPlanet1:
                case GameVersion.LittleBigPlanetPSP:
                case GameVersion.Unknown:
                default: // The rest do not support custom earths.
                {
                    throw new ArgumentException($"invalid gameVersion {gameToken.GameVersion} for setting earth");
                }
            }
        }

        if (update.Location != null)
        {
            Location? loc = await this.database.Locations.FirstOrDefaultAsync(l => l.Id == user.LocationId);
            if (loc == null) throw new Exception("User loc is null, this should never happen.");

            loc.X = update.Location.X;
            loc.Y = update.Location.Y;
        }

        if (this.database.ChangeTracker.HasChanges()) await this.database.SaveChangesAsync();
        return this.Ok();
    }

    [HttpPost("update_my_pins")]
    public async Task<IActionResult> UpdateMyPins()
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        string pinsString = await new StreamReader(this.Request.Body).ReadToEndAsync();
        Pins? pinJson = JsonSerializer.Deserialize<Pins>(pinsString);
        if (pinJson == null) return this.BadRequest();

        // Sometimes the update gets called periodically as pin progress updates via playing,
        // may not affect equipped profile pins however, so check before setting it.
        string currentPins = user.Pins;
        string newPins = string.Join(",", pinJson.ProfilePins);

        if (string.Equals(currentPins, newPins)) return this.Ok("[{\"StatusCode\":200}]");

        user.Pins = newPins;
        await this.database.SaveChangesAsync();

        return this.Ok("[{\"StatusCode\":200}]");
    }
}
