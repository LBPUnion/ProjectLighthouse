#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Resources;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class PublishController : ControllerBase
{
    private readonly DatabaseContext database;

    public PublishController(DatabaseContext database)
    {
        this.database = database;
    }

    /// <summary>
    ///     Endpoint the game uses to check what resources need to be uploaded and if the level can be uploaded
    /// </summary>
    [HttpPost("startPublish")]
    public async Task<IActionResult> StartPublish()
    {
        GameTokenEntity token = this.GetToken();

        UserEntity? user = await this.database.UserFromGameToken(token);
        if (user == null) return this.Forbid();

        GameUserSlot? slot = await this.DeserializeBody<GameUserSlot>();
        if (slot == null)
        {
            Logger.Warn("Rejecting level upload, slot is null", LogArea.Publish);
            return this.BadRequest(); // if the level cant be parsed then it obviously cant be uploaded
        }

        if (string.IsNullOrEmpty(slot.RootLevel))
        {
            Logger.Warn("Rejecting level upload, slot does not include rootLevel", LogArea.Publish);
            return this.BadRequest();
        }

        if (slot.Resources?.Length == 0) slot.Resources = new[]{slot.RootLevel,};

        if (slot.Resources == null)
        {
            Logger.Warn("Rejecting level upload, resource list is null", LogArea.Publish);
            return this.BadRequest();
        }

        int usedSlots = await this.database.Slots.CountAsync(s => s.CreatorId == token.UserId && s.GameVersion == token.GameVersion);

        // Republish logic
        if (slot.SlotId != 0)
        {
            SlotEntity? oldSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slot.SlotId);
            if (oldSlot == null)
            {
                Logger.Warn("Rejecting level republish, could not find old slot", LogArea.Publish);
                return this.NotFound();
            }
            if (oldSlot.CreatorId != user.UserId)
            {
                Logger.Warn("Rejecting level republish, old slot's creator is not publishing user", LogArea.Publish);
                return this.BadRequest();
            }
        }
        else if (usedSlots > user.EntitledSlots)
        {
            return this.Forbid();
        }

        HashSet<string> resources = new(slot.Resources)
        {
            slot.IconHash,
        };
        resources = resources.Where(hash => !FileHelper.ResourceExists(hash)).ToHashSet();

        return this.Ok(new SlotResourceResponse(resources.ToList()));
    }

    /// <summary>
    ///     Endpoint actually used to publish a level
    /// </summary>
    [HttpPost("publish")]
    public async Task<IActionResult> Publish([FromQuery] string? game)
    {
        GameTokenEntity token = this.GetToken();

        UserEntity? user = await this.database.UserFromGameToken(token);
        if (user == null) return this.Forbid();

        GameUserSlot? slot = await this.DeserializeBody<GameUserSlot>();

        if (slot == null)
        {
            Logger.Warn("Rejecting level upload, slot is null", LogArea.Publish);
            return this.BadRequest();
        }

        if (slot.Resources == null)
        {
            Logger.Warn("Rejecting level upload, resource list is null", LogArea.Publish);
            return this.BadRequest();
        }

        slot.Description = CensorHelper.FilterMessage(slot.Description);

        if (slot.Description.Length > 512)
        {
            Logger.Warn($"Rejecting level upload, description too long ({slot.Description.Length} characters)", LogArea.Publish);
            return this.BadRequest();
        }

        slot.Name = CensorHelper.FilterMessage(slot.Name);

        if (slot.Name.Length > 64)
        {
            Logger.Warn($"Rejecting level upload, title too long ({slot.Name.Length} characters)", LogArea.Publish);
            return this.BadRequest();
        }

        if (slot.Resources != null && slot.Resources.Any(resource => !FileHelper.ResourceExists(resource)))
        {
            Logger.Warn("Rejecting level upload, missing resource(s)", LogArea.Publish);
            return this.BadRequest();
        }

        LbpFile? rootLevel = LbpFile.FromHash(slot.RootLevel);

        if (rootLevel == null)
        {
            Logger.Warn("Rejecting level upload, unable to find rootLevel", LogArea.Publish);
            return this.BadRequest();
        }

        if (!slot.IsAdventurePlanet)
        {
            if (rootLevel.FileType != LbpFileType.Level)
            {
                Logger.Warn("Rejecting level upload, rootLevel is not a level", LogArea.Publish);
                return this.BadRequest();
            }
        }
        else
        {
            if (rootLevel.FileType != LbpFileType.Adventure)
            {
                Logger.Warn("Rejecting level upload, rootLevel is not a LBP 3 Adventure", LogArea.Publish);
                return this.BadRequest();
            }      
        }

        GameVersion slotVersion = FileHelper.ParseLevelVersion(rootLevel);

        slot.GameVersion = slotVersion;
        if (slotVersion == GameVersion.Unknown) slot.GameVersion = token.GameVersion;

        slot.AuthorLabels = LabelHelper.RemoveInvalidLabels(slot.AuthorLabels);

        // Republish logic
        if (slot.SlotId != 0)
        {
            SlotEntity? oldSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slot.SlotId);
            if (oldSlot == null)
            {
                Logger.Warn("Rejecting level republish, wasn't able to find old slot", LogArea.Publish);
                return this.NotFound();
            }

            if (oldSlot.CreatorId != user.UserId)
            {
                Logger.Warn("Rejecting level republish, old level not owned by current user", LogArea.Publish);
                return this.BadRequest();
            }

            // I hate lbp3
            if (game != null)
            {
                GameVersion intendedVersion = FromAbbreviation(game);
                if (intendedVersion != GameVersion.Unknown && intendedVersion != slotVersion)
                {
                    // Delete the useless rootLevel that lbp3 just uploaded
                    if (slotVersion == GameVersion.LittleBigPlanet3)
                        FileHelper.DeleteResource(slot.RootLevel);
                    else
                    {
                        oldSlot.GameVersion = slot.GameVersion;
                        oldSlot.RootLevel = slot.RootLevel;
                        oldSlot.Resources = slot.Resources;
                    }
                }
            }

            oldSlot.Name = slot.Name;
            oldSlot.Description = slot.Description;
            oldSlot.Location = slot.Location;
            oldSlot.IconHash = slot.IconHash;
            oldSlot.BackgroundHash = slot.BackgroundHash;
            oldSlot.AuthorLabels = slot.AuthorLabels;
            oldSlot.Shareable = slot.IsShareable;
            oldSlot.Resources = slot.Resources;
            oldSlot.InitiallyLocked = slot.InitiallyLocked;
            oldSlot.Lbp1Only = slot.IsLbp1Only;
            oldSlot.IsAdventurePlanet = slot.IsAdventurePlanet;
            oldSlot.LevelType = slot.LevelType;
            oldSlot.SubLevel = slot.IsSubLevel;
            oldSlot.MoveRequired = slot.IsMoveRequired;
            oldSlot.CrossControllerRequired = slot.IsCrossControlRequired;

            oldSlot.LastUpdated = TimeHelper.TimestampMillis;

            if (slot.MinimumPlayers == 0 || slot.MaximumPlayers == 0)
            {
                slot.MinimumPlayers = 1;
                slot.MaximumPlayers = 4;
            }

            oldSlot.MinimumPlayers = Math.Clamp(slot.MinimumPlayers, 1, 4);
            oldSlot.MaximumPlayers = Math.Clamp(slot.MaximumPlayers, 1, 4);

            await this.database.SaveChangesAsync();
            return this.Ok(SlotBase.CreateFromEntity(oldSlot, this.GetToken()));
        }

        int usedSlots = await this.database.Slots.CountAsync(s => s.CreatorId == token.UserId && s.GameVersion == slotVersion);

        if (usedSlots > user.EntitledSlots)
        {
            Logger.Warn("Rejecting level upload, too many published slots", LogArea.Publish);
            return this.BadRequest();
        }

        SlotEntity slotEntity = SlotBase.ConvertToEntity(slot);

        slot.CreatorId = user.UserId;
        slot.FirstUploaded = TimeHelper.TimestampMillis;
        slot.LastUpdated = TimeHelper.TimestampMillis;

        if (slot.MinimumPlayers == 0 || slot.MaximumPlayers == 0)
        {
            slot.MinimumPlayers = 1;
            slot.MaximumPlayers = 4;
        }

        slot.MinimumPlayers = Math.Clamp(slot.MinimumPlayers, 1, 4);
        slot.MaximumPlayers = Math.Clamp(slot.MaximumPlayers, 1, 4);

        this.database.Slots.Add(slotEntity);
        await this.database.SaveChangesAsync();

        if (user.LevelVisibility == PrivacyType.All)
        {
            await WebhookHelper.SendWebhook("New level published!",
                $"**{user.Username}** just published a new level: [**{slot.Name}**]({ServerConfiguration.Instance.ExternalUrl}/slot/{slot.SlotId})\n{slot.Description}");
        }

        Logger.Success($"Successfully published level {slot.Name} (id: {slot.SlotId}) by {user.Username} (id: {user.UserId})", LogArea.Publish);

        return this.Ok(SlotBase.CreateFromEntity(slotEntity, this.GetToken()));
    }

    [HttpPost("unpublish/{id:int}")]
    public async Task<IActionResult> Unpublish(int id)
    {
        GameTokenEntity token = this.GetToken();

        SlotEntity? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        if (slot.CreatorId != token.UserId) return this.Forbid();

        this.database.Slots.Remove(slot);

        await this.database.SaveChangesAsync();

        return this.Ok();
    }

    private static GameVersion FromAbbreviation(string abbr)
    {
        return abbr switch
        {
            "lbp1" => GameVersion.LittleBigPlanet1,
            "lbp2" => GameVersion.LittleBigPlanet2,
            "lbp3" => GameVersion.LittleBigPlanet3,
            "lbpv" => GameVersion.LittleBigPlanetVita,
            "lbppsp" => GameVersion.LittleBigPlanetPSP,
            _ => GameVersion.Unknown,
        };
    }
}