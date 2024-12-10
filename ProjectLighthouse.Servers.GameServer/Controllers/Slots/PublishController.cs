using System.Diagnostics;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Filter;
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

        // Deny request if in read-only mode
        if (ServerConfiguration.Instance.UserGeneratedContentLimits.ReadOnlyMode) return this.BadRequest();

        GameUserSlot? slot = await this.DeserializeBody<GameUserSlot>();
        if (slot == null)
        {
            Logger.Warn("Rejecting level upload, slot is null", LogArea.Publish);
            await this.database.SendNotification(user.UserId,
                "Your level failed to publish. (LH-PUB-0001)");
            return this.BadRequest(); // if the level cant be parsed then it obviously cant be uploaded
        }

        if (string.IsNullOrEmpty(slot.RootLevel))
        {
            Logger.Warn("Rejecting level upload, slot does not include rootLevel", LogArea.Publish);
            await this.database.SendNotification(user.UserId,
                $"{slot.Name} failed to publish. (LH-PUB-0002)");
            return this.BadRequest();
        }

        if (slot.Resources?.Length == 0) slot.Resources = new[]{slot.RootLevel,};

        if (slot.Resources == null)
        {
            Logger.Warn("Rejecting level upload, resource list is null", LogArea.Publish);
            await this.database.SendNotification(user.UserId,
                $"{slot.Name} failed to publish. (LH-PUB-0003)");
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
                await this.database.SendNotification(user.UserId,
                    $"{slot.Name} failed to republish. (LH-REP-0001)");
                return this.NotFound();
            }
            if (oldSlot.CreatorId != user.UserId)
            {
                Logger.Warn("Rejecting level republish, old slot's creator is not publishing user", LogArea.Publish);
                await this.database.SendNotification(user.UserId,
                    $"{slot.Name} failed to republish because you are not the original publisher. (LH-REP-0002)");
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

        // Deny request if in read-only mode
        if (ServerConfiguration.Instance.UserGeneratedContentLimits.ReadOnlyMode) return this.BadRequest();

        GameUserSlot? slot = await this.DeserializeBody<GameUserSlot>();

        if (slot == null)
        {
            Logger.Warn("Rejecting level upload, slot is null", LogArea.Publish);
            await this.database.SendNotification(user.UserId,
                "Your level failed to publish. (LH-PUB-0001)");
            return this.BadRequest();
        }

        if (slot.Resources?.Length == 0)
        {
            Logger.Warn("Rejecting level upload, resource list is null", LogArea.Publish);
            await this.database.SendNotification(user.UserId,
                $"{slot.Name} failed to publish. (LH-PUB-0003)");
            return this.BadRequest();
        }
        // Yes Rider, this isn't null
        Debug.Assert(slot.Resources != null, "slot.ResourceList != null");

        slot.Name = CensorHelper.FilterMessage(slot.Name, FilterLocation.SlotName, user.Username);

        if (slot.Name.Length > 64)
        {
            Logger.Warn($"Rejecting level upload, title too long ({slot.Name.Length} characters)",
                LogArea.Publish);
            await this.database.SendNotification(user.UserId,
                $"{slot.Name} failed to publish because the name is too long, {slot.Name.Length} characters. (LH-PUB-0004)");
            return this.BadRequest();
        }

        slot.Description = CensorHelper.FilterMessage(slot.Description, FilterLocation.SlotDescription, user.Username);

        if (slot.Description.Length > 512)
        {
            Logger.Warn($"Rejecting level upload, description too long ({slot.Description.Length} characters)",
                LogArea.Publish);
            await this.database.SendNotification(user.UserId,
                $"{slot.Name} failed to publish because the description is too long, {slot.Description.Length} characters. (LH-PUB-0005)");
            return this.BadRequest();
        }

        if (!GameResourceHelper.IsValidTexture(slot.IconHash))
        {
            Logger.Warn("Rejecting level upload, invalid icon resource", LogArea.Publish);
            await this.database.SendNotification(user.UserId,
                $"{slot.Name} failed to publish because your level icon is invalid. (LH-PUB-0010)");
            return this.BadRequest();
        }

        if (slot.Resources.Any(resource => !FileHelper.ResourceExists(resource)))
        {
            Logger.Warn("Rejecting level upload, missing resource(s)", LogArea.Publish);
            await this.database.SendNotification(user.UserId,
                $"{slot.Name} failed to publish because the server is missing resources. (LH-PUB-0006)");
            return this.BadRequest();
        }

        LbpFile? rootLevel = LbpFile.FromHash(slot.RootLevel);

        if (rootLevel == null)
        {
            Logger.Warn("Rejecting level upload, unable to find rootLevel", LogArea.Publish);
            await this.database.SendNotification(user.UserId,
                $"{slot.Name} failed to publish. (LH-PUB-0002)");
            return this.BadRequest();
        }

        if (!slot.IsAdventurePlanet)
        {
            if (rootLevel.FileType != LbpFileType.Level)
            {
                Logger.Warn("Rejecting level upload, rootLevel is not a level", LogArea.Publish);
                await this.database.SendNotification(user.UserId,
                    $"{slot.Name} failed to publish. (LH-PUB-0007)");
                return this.BadRequest();
            }
        }
        else
        {
            if (rootLevel.FileType != LbpFileType.Adventure)
            {
                Logger.Warn("Rejecting level upload, rootLevel is not a LBP 3 Adventure", LogArea.Publish);
                await this.database.SendNotification(user.UserId,
                    $"{slot.Name} failed to publish. (LH-PUB-0008)");
                return this.BadRequest();
            }
        }

        GameVersion slotVersion = FileHelper.ParseLevelVersion(rootLevel);

        slot.GameVersion = slotVersion;
        if (slotVersion == GameVersion.Unknown) slot.GameVersion = token.GameVersion;

        slot.AuthorLabels = LabelHelper.RemoveInvalidLabels(slot.AuthorLabels);

        if (!slot.Resources.Contains(slot.RootLevel))
            slot.Resources = slot.Resources.Append(rootLevel.Hash).ToArray();

        string resourceCollection = string.Join(",", slot.Resources);

        SlotEntity? oldSlot = null;
        if (slot.SlotId != 0)
        {
            oldSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slot.SlotId);
        }

        // Republish logic
        if (oldSlot != null)
        {
            if (oldSlot.CreatorId != user.UserId)
            {
                Logger.Warn("Rejecting level republish, old level not owned by current user", LogArea.Publish);
                await this.database.SendNotification(user.UserId,
                    $"{slot.Name} failed to republish because you are not the original publisher. (LH-REP-0002)");
                return this.BadRequest();
            }

            // This is a workaround to prevent lbp3 from overwriting the rootLevel of older levels
            // For some reason when republishing in lbp3 it automatically converts the level to lbp3
            // so it must be handled here. The game query is only sent by lbp3 so it can be safely assumed
            // that if it is present, then the level must be be checked for conversion
            GameVersion intendedVersion = game != null ? FromAbbreviation(game) : slot.GameVersion;
            if (intendedVersion != GameVersion.Unknown && intendedVersion == slot.GameVersion)
            {
                oldSlot.GameVersion = slot.GameVersion;
                oldSlot.RootLevel = rootLevel.Hash;
                oldSlot.ResourceCollection = resourceCollection;
            }
            else
            {
                Logger.Warn(
                    $"Slot rootLevel divergence: game={game}, slotVersion={slot.GameVersion}, intendedVersion={intendedVersion}, oldVersion={oldSlot.GameVersion}",
                    LogArea.Publish);
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

            // Check if the level has been locked by a moderator to avoid unlocking it
            if (oldSlot.LockedByModerator && !slot.InitiallyLocked)
            {
                await this.database.SendNotification(user.UserId,
                    $"{slot.Name} will not be unlocked because it has been locked by a moderator. (LH-REP-0003)");
                oldSlot.InitiallyLocked = true;
            }

            await this.database.SaveChangesAsync();
            return this.Ok(SlotBase.CreateFromEntity(oldSlot, token));
        }

        int usedSlots = await this.database.Slots.CountAsync(s => s.CreatorId == token.UserId && s.GameVersion == slotVersion);

        if (usedSlots > user.EntitledSlots)
        {
            Logger.Warn("Rejecting level upload, too many published slots", LogArea.Publish);
            await this.database.SendNotification(user.UserId,
                $"{slot.Name} failed to publish because you have reached the maximum number of levels on your earth. (LH-PUB-0009)");
            return this.BadRequest();
        }

        SlotEntity slotEntity = SlotBase.ConvertToEntity(slot);
        slotEntity.CreatorId = user.UserId;
        slotEntity.FirstUploaded = TimeHelper.TimestampMillis;
        slotEntity.LastUpdated = TimeHelper.TimestampMillis;
        slotEntity.ResourceCollection = resourceCollection;

        if (slotEntity.MinimumPlayers == 0 || slot.MaximumPlayers == 0)
        {
            slotEntity.MinimumPlayers = 1;
            slotEntity.MaximumPlayers = 4;
        }

        slotEntity.MinimumPlayers = Math.Clamp(slotEntity.MinimumPlayers, 1, 4);
        slotEntity.MaximumPlayers = Math.Clamp(slotEntity.MaximumPlayers, 1, 4);

        this.database.Slots.Add(slotEntity);
        await this.database.SaveChangesAsync();

        if (user.LevelVisibility == PrivacyType.All)
        {
            await WebhookHelper.SendWebhook("New level published!",
                $"**{user.Username}** just published a new level: [**{slotEntity.Name}**]({ServerConfiguration.Instance.ExternalUrl}/slot/{slotEntity.SlotId})\n{slotEntity.Description}");
        }

        Logger.Success($"Successfully published level {slotEntity.Name} (id: {slotEntity.SlotId}) by {user.Username} (id: {user.UserId})", LogArea.Publish);

        return this.Ok(SlotBase.CreateFromEntity(slotEntity, token));
    }

    [HttpPost("unpublish/{id:int}")]
    public async Task<IActionResult> Unpublish(int id)
    {
        GameTokenEntity token = this.GetToken();

        // Deny request if in read-only mode
        if (ServerConfiguration.Instance.UserGeneratedContentLimits.ReadOnlyMode) return this.BadRequest();

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