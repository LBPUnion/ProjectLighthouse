#nullable enable
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class PublishController : ControllerBase
{
    private readonly Database database;

    public PublishController(Database database)
    {
        this.database = database;
    }

    /// <summary>
    ///     Endpoint the game uses to check what resources need to be uploaded and if the level can be uploaded
    /// </summary>
    [HttpPost("startPublish")]
    public async Task<IActionResult> StartPublish()
    {
        (User, GameToken)? userAndToken = await this.database.UserAndGameTokenFromRequest(this.Request);

        if (userAndToken == null) return this.StatusCode(403, "");

        // ReSharper disable once PossibleInvalidOperationException
        User user = userAndToken.Value.Item1;
        GameToken gameToken = userAndToken.Value.Item2;

        Slot? slot = await this.getSlotFromBody();
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

        if (string.IsNullOrEmpty(slot.ResourceCollection)) slot.ResourceCollection = slot.RootLevel;

        // Republish logic
        if (slot.SlotId != 0)
        {
            Slot? oldSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slot.SlotId);
            if (oldSlot == null)
            {
                Logger.Warn("Rejecting level reupload, could not find old slot", LogArea.Publish);
                return this.NotFound();
            }
            if (oldSlot.CreatorId != user.UserId)
            {
                Logger.Warn("Rejecting level reupload, old slot's creator is not publishing user", LogArea.Publish);
                return this.BadRequest();
            }
        }
        else if (user.GetUsedSlotsForGame(gameToken.GameVersion) > user.EntitledSlots)
        {
            return this.StatusCode(403, "");
        }

        slot.ResourceCollection += "," + slot.IconHash; // tells LBP to upload icon after we process resources here

        string resources = slot.Resources.Where
                (hash => !FileHelper.ResourceExists(hash))
            .Aggregate("", (current, hash) => current + LbpSerializer.StringElement("resource", hash));

        return this.Ok(LbpSerializer.TaggedStringElement("slot", resources, "type", "user"));
    }

    /// <summary>
    ///     Endpoint actually used to publish a level
    /// </summary>
    [HttpPost("publish")]
    public async Task<IActionResult> Publish()
    {
        (User, GameToken)? userAndToken = await this.database.UserAndGameTokenFromRequest(this.Request);

        if (userAndToken == null) return this.StatusCode(403, "");

        // ReSharper disable once PossibleInvalidOperationException
        User user = userAndToken.Value.Item1;
        GameToken gameToken = userAndToken.Value.Item2;
        Slot? slot = await this.getSlotFromBody();

        if (slot == null)
        {
            Logger.Warn("Rejecting level upload, slot is null", LogArea.Publish);
            return this.BadRequest();
        }

        if (slot.Location == null)
        {
            Logger.Warn("Rejecting level upload, slot location is null", LogArea.Publish);
            return this.BadRequest();
        }

        if (slot.Description.Length > 512)
        {
            Logger.Warn($"Rejecting level upload, description too long ({slot.Description.Length} characters)", LogArea.Publish);
            return this.BadRequest();
        }

        if (slot.Name.Length > 64)
        {
            Logger.Warn($"Rejecting level upload, title too long ({slot.Name.Length} characters)", LogArea.Publish);
            return this.BadRequest();
        }

        if (slot.Resources.Any(resource => !FileHelper.ResourceExists(resource)))
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
        if (slotVersion == GameVersion.Unknown) slot.GameVersion = gameToken.GameVersion;

        slot.AuthorLabels = LabelHelper.RemoveInvalidLabels(slot.AuthorLabels);

        // Republish logic
        if (slot.SlotId != 0)
        {
            Slot? oldSlot = await this.database.Slots.Include(s => s.Location).FirstOrDefaultAsync(s => s.SlotId == slot.SlotId);
            if (oldSlot == null)
            {
                Logger.Warn("Rejecting level republish, wasn't able to find old slot", LogArea.Publish);
                return this.NotFound();
            }

            if (oldSlot.Location == null) throw new ArgumentNullException();

            if (oldSlot.CreatorId != user.UserId)
            {
                Logger.Warn("Rejecting level republish, old level not owned by current user", LogArea.Publish);
                return this.BadRequest();
            }

            oldSlot.Location.X = slot.Location.X;
            oldSlot.Location.Y = slot.Location.Y;

            slot.CreatorId = oldSlot.CreatorId;
            slot.LocationId = oldSlot.LocationId;
            slot.SlotId = oldSlot.SlotId;

            #region Set plays

            slot.PlaysLBP1 = oldSlot.PlaysLBP1;
            slot.PlaysLBP1Complete = oldSlot.PlaysLBP1Complete;
            slot.PlaysLBP1Unique = oldSlot.PlaysLBP1Unique;

            slot.PlaysLBP2 = oldSlot.PlaysLBP2;
            slot.PlaysLBP2Complete = oldSlot.PlaysLBP2Complete;
            slot.PlaysLBP2Unique = oldSlot.PlaysLBP2Unique;

            slot.PlaysLBP3 = oldSlot.PlaysLBP3;
            slot.PlaysLBP3Complete = oldSlot.PlaysLBP3Complete;
            slot.PlaysLBP3Unique = oldSlot.PlaysLBP3Unique;

            #endregion

            slot.FirstUploaded = oldSlot.FirstUploaded;
            slot.LastUpdated = TimeHelper.UnixTimeMilliseconds();

            slot.TeamPick = oldSlot.TeamPick;

            if (slot.MinimumPlayers == 0 || slot.MaximumPlayers == 0)
            {
                slot.MinimumPlayers = 1;
                slot.MaximumPlayers = 4;
            }

            this.database.Entry(oldSlot).CurrentValues.SetValues(slot);
            await this.database.SaveChangesAsync();
            return this.Ok(oldSlot.Serialize(gameToken.GameVersion));
        }

        if (user.GetUsedSlotsForGame(slotVersion) > user.EntitledSlots)
        {
            Logger.Warn("Rejecting level upload, too many published slots", LogArea.Publish);
            return this.BadRequest();
        }

        //TODO: parse location in body
        Location l = new()
        {
            X = slot.Location.X,
            Y = slot.Location.Y,
        };
        this.database.Locations.Add(l);
        await this.database.SaveChangesAsync();
        slot.LocationId = l.Id;
        slot.CreatorId = user.UserId;
        slot.FirstUploaded = TimeHelper.UnixTimeMilliseconds();
        slot.LastUpdated = TimeHelper.UnixTimeMilliseconds();

        if (slot.MinimumPlayers == 0 || slot.MaximumPlayers == 0)
        {
            slot.MinimumPlayers = 1;
            slot.MaximumPlayers = 4;
        }

        this.database.Slots.Add(slot);
        await this.database.SaveChangesAsync();

        if (user.LevelVisibility == PrivacyType.All)
        {
            await WebhookHelper.SendWebhook("New level published!",
                $"**{user.Username}** just published a new level: [**{slot.Name}**]({ServerConfiguration.Instance.ExternalUrl}/slot/{slot.SlotId})\n{slot.Description}");
        }

        Logger.Success($"Successfully published level {slot.Name} (id: {slot.SlotId}) by {user.Username} (id: {user.UserId})", LogArea.Publish);

        return this.Ok(slot.Serialize(gameToken.GameVersion));
    }

    [HttpPost("unpublish/{id:int}")]
    public async Task<IActionResult> Unpublish(int id)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.Include(s => s.Location).FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        if (slot.Location == null) throw new ArgumentNullException();

        if (slot.CreatorId != token.UserId) return this.StatusCode(403, "");

        this.database.Locations.Remove(slot.Location);
        this.database.Slots.Remove(slot);

        await this.database.SaveChangesAsync();

        return this.Ok();
    }

    private async Task<Slot?> getSlotFromBody()
    {
        this.Request.Body.Position = 0;
        string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

        XmlSerializer serializer = new(typeof(Slot));
        Slot? slot = (Slot?)serializer.Deserialize(new StringReader(bodyString));

        SanitizationHelper.SanitizeStringsInClass(slot);

        return slot;
    }
}