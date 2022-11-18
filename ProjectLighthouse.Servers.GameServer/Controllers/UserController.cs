#nullable enable
using System.Text.Json;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

[ApiController]
[Authorize]
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
            })
            .FirstOrDefaultAsync();
        if (partialUser == null) return null;

        string user = LbpSerializer.TaggedStringElement("npHandle", partialUser.Username, "icon", partialUser.IconHash);
        return LbpSerializer.TaggedStringElement("user", user, "type", "user");
    }

    [HttpGet("user/{username}")]
    public async Task<IActionResult> GetUser(string username)
    {
        GameToken token = this.GetToken();

        string? user = await this.getSerializedUser(username, token.GameVersion);
        if (user == null) return this.NotFound();

        return this.Ok(user);
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUserAlt([FromQuery] string[] u)
    {
        List<string?> serializedUsers = new();
        foreach (string userId in u) serializedUsers.Add(await this.getSerializedUserPicture(userId));

        string serialized = serializedUsers.Aggregate(string.Empty, (current, user) => user == null ? current : current + user);

        return this.Ok(LbpSerializer.StringElement("users", serialized));
    }

    [HttpPost("updateUser")]
    public async Task<IActionResult> UpdateUser()
    {
        GameToken token = this.GetToken();

        User? user = await this.database.UserFromGameToken(token);
        if (user == null) return this.StatusCode(403, "");

        UserUpdate? update = await this.DeserializeBody<UserUpdate>("updateUser", "user");

        if (update == null) return this.BadRequest();

        SanitizationHelper.SanitizeStringsInClass(update);

        if (update.Biography != null)
        {
            if (update.Biography.Length > 512) return this.BadRequest();

            user.Biography = update.Biography;
        }

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (string? resource in new[]{update.IconHash, update.YayHash, update.MehHash, update.BooHash, update.PlanetHash,})
        {
            if (resource == "0") continue;

            if (resource != null && !resource.StartsWith('g') && !FileHelper.ResourceExists(resource))
            {
                return this.BadRequest();
            }
        }

        if (update.IconHash != null) user.IconHash = update.IconHash;

        if (update.YayHash != null) user.YayHash = update.YayHash;

        if (update.MehHash != null) user.MehHash = update.MehHash;

        if (update.BooHash != null) user.BooHash = update.BooHash;

        if (update.Slots != null)
        {
            foreach (UserUpdateSlot? updateSlot in update.Slots)
            {
                // ReSharper disable once MergeIntoNegatedPattern
                if (updateSlot.Type != SlotType.User || updateSlot.Location == null || updateSlot.SlotId == 0) continue;

                Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == updateSlot.SlotId);
                if (slot == null) continue;

                if (slot.CreatorId != token.UserId) continue;

                Location? loc = await this.database.Locations.FirstOrDefaultAsync(l => l.Id == slot.LocationId);

                if (loc == null) throw new ArgumentNullException();

                loc.X = updateSlot.Location.X;
                loc.Y = updateSlot.Location.Y;
            }
        }

        if (update.PlanetHashLBP2CC != null) user.PlanetHashLBP2CC = update.PlanetHashLBP2CC;

        if (update.PlanetHash != null)
        {
            switch (token.GameVersion)
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
                    throw new ArgumentException($"invalid gameVersion {token.GameVersion} for setting earth");
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
        User? user = await this.database.UserFromGameToken(this.GetToken());
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