#nullable enable
using System.Text.Json;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Helpers;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Users;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;
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
    private readonly DatabaseContext database;

    public UserController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpGet("user/{username}")]
    public async Task<IActionResult> GetUser(string username)
    {
        UserEntity? user = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (user == null) return this.NotFound();

        return this.Ok(GameUser.CreateFromEntity(user, this.GetToken().GameVersion));
    }

    [HttpGet("users")]
    public async Task<IActionResult> GetUserAlt([FromQuery(Name = "u")] string[] userList)
    {
        List<MinimalUserProfile> minimalUserList = new();
        foreach (string username in userList)
        {
            MinimalUserProfile? profile = await this.database.Users.Where(u => u.Username == username)
                .Select(u => new MinimalUserProfile
                {
                    UserHandle = new NpHandle(u.Username, u.IconHash),
                })
                .FirstOrDefaultAsync();
            if (profile == null) continue;
            minimalUserList.Add(profile);
        }

        return this.Ok(new MinimalUserListResponse(minimalUserList));
    }

    [HttpPost("updateUser")]
    public async Task<IActionResult> UpdateUser()
    {
        GameTokenEntity token = this.GetToken();

        UserEntity? user = await this.database.UserFromGameToken(token);
        if (user == null) return this.Forbid();

        UserUpdate? update = await this.DeserializeBody<UserUpdate>("updateUser", "user");

        if (update == null) return this.BadRequest();

        if (update.Biography != null)
        {
            if (update.Biography.Length > 512) return this.BadRequest();

            user.Biography = update.Biography;
        }

        if (update.Location != null) user.Location = update.Location;

        // ReSharper disable once LoopCanBeConvertedToQuery
        foreach (string? resource in new[]{update.IconHash, update.YayHash, update.MehHash, update.BooHash, update.PlanetHash,})
        {
            if (string.IsNullOrWhiteSpace(resource)) continue;

            if (!FileHelper.ResourceExists(resource) && !resource.StartsWith('g')) return this.BadRequest();

            if (!GameResourceHelper.IsValidTexture(resource)) return this.BadRequest();
        }

        if (update.IconHash != null) user.IconHash = update.IconHash;

        if (update.YayHash != null) user.YayHash = update.YayHash;

        if (update.MehHash != null) user.MehHash = update.MehHash;

        if (update.BooHash != null) user.BooHash = update.BooHash;

        if (update.Slots != null)
        {
            update.Slots = update.Slots.Where(s => s.Type == SlotType.User)
                .Where(s => s.Location != null)
                .Where(s => s.SlotId != 0).ToList();
            foreach (UserUpdateSlot? updateSlot in update.Slots)
            {
                SlotEntity? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == updateSlot.SlotId);
                if (slot == null) continue;

                if (slot.CreatorId != token.UserId) continue;

                slot.Location = updateSlot.Location!;
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
                    string bodyString = await this.ReadBodyAsync();
                    Logger.Warn($"User with invalid gameVersion '{token.GameVersion}' tried to set earth hash: \n" +
                                $"body: '{bodyString}'",
                        LogArea.Resources);
                    break;
                }
            }
        }

        await this.database.SaveChangesAsync();

        return this.Ok();
    }

    [HttpPost("update_my_pins")]
    [Produces("text/json")]
    public async Task<IActionResult> UpdateMyPins()
    {
        UserEntity? user = await this.database.UserFromGameToken(this.GetToken());
        if (user == null) return this.Forbid();

        string bodyString = await this.ReadBodyAsync();

        Pins? pinJson = JsonSerializer.Deserialize<Pins>(bodyString);
        if (pinJson?.ProfilePins == null) return this.BadRequest();

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