#nullable enable
using Discord;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Resources;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class PhotosController : ControllerBase
{
    private readonly DatabaseContext database;

    public PhotosController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpPost("uploadPhoto")]
    public async Task<IActionResult> UploadPhoto()
    {
        GameTokenEntity token = this.GetToken();

        int photoCount = await this.database.Photos.CountAsync(p => p.CreatorId == token.UserId);
        if (photoCount >= ServerConfiguration.Instance.UserGeneratedContentLimits.PhotosQuota) return this.BadRequest();

        GamePhoto? photo = await this.DeserializeBody<GamePhoto>();
        if (photo == null) return this.BadRequest();

        foreach (PhotoEntity p in this.database.Photos.Where(p => p.CreatorId == token.UserId))
        {
            if (p.LargeHash == photo.LargeHash) return this.Ok(); // photo already uploaded
            if (p.MediumHash == photo.MediumHash) return this.Ok();
            if (p.SmallHash == photo.SmallHash) return this.Ok();
            if (p.PlanHash == photo.PlanHash) return this.Ok();
        }

        PhotoEntity photoEntity = new()
        {
            CreatorId = token.UserId,
            SmallHash = photo.SmallHash,
            MediumHash = photo.MediumHash,
            LargeHash = photo.LargeHash,
            PlanHash = photo.PlanHash,
            Timestamp = photo.Timestamp,
        };

        if (photo.LevelInfo?.RootLevel != null)
        {
            bool validLevel = false;
            PhotoSlot photoSlot = photo.LevelInfo;
            if (photoSlot.SlotType is SlotType.Pod or SlotType.Local) photoSlot.SlotId = 0;
            switch (photoSlot.SlotType)
            {
                case SlotType.User:
                {
                    // We'll grab the slot by the RootLevel and see what happens from here.
                    SlotEntity? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.Type == SlotType.User && s.ResourceCollection.Contains(photoSlot.RootLevel));
                    if (slot == null) break;

                    if (!string.IsNullOrEmpty(slot.RootLevel)) validLevel = true;
                    if (slot.IsAdventurePlanet) photoSlot.SlotId = slot.SlotId;
                    break;
                }
                case SlotType.Pod:
                case SlotType.Local:
                case SlotType.Developer:
                {
                    SlotEntity? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.Type == photoSlot.SlotType && s.InternalSlotId == photoSlot.SlotId);
                    if (slot != null) 
                        photoSlot.SlotId = slot.SlotId;
                    else
                        photoSlot.SlotId = await SlotHelper.GetPlaceholderSlotId(this.database, photoSlot.SlotId, photoSlot.SlotType);
                    validLevel = true;
                    break;
                }
                case SlotType.Moon:
                case SlotType.Unknown:
                case SlotType.Unknown2:
                case SlotType.DLC:
                default: Logger.Warn($"Invalid photo level type: {photoSlot.SlotType}", LogArea.Photos);
                    break;
            }

            if (validLevel) photoEntity.SlotId = photoSlot.SlotId;
        }

        if (photo.Subjects?.Count > 4) return this.BadRequest();

        if (photo.Timestamp > TimeHelper.Timestamp) photoEntity.Timestamp = TimeHelper.Timestamp;

        this.database.Photos.Add(photoEntity);

        // Save to get photo ID for the PhotoSubject foreign keys
        await this.database.SaveChangesAsync();

        if (photo.Subjects != null)
        {
            // Check for duplicate photo subjects
            List<string> subjectUserIds = new(4);
            foreach (GamePhotoSubject subject in photo.Subjects)
            {
                if (subjectUserIds.Contains(subject.Username) && !string.IsNullOrEmpty(subject.Username))
                    return this.BadRequest();

                subjectUserIds.Add(subject.Username);
            }

            foreach (GamePhotoSubject subject in photo.Subjects.Where(subject => !string.IsNullOrEmpty(subject.Username)))
            {
                subject.UserId = await this.database.Users.Where(u => u.Username == subject.Username)
                    .Select(u => u.UserId)
                    .FirstOrDefaultAsync();

                if (subject.UserId == 0) continue;

                PhotoSubjectEntity subjectEntity = new()
                {
                    PhotoId = photoEntity.PhotoId,
                    UserId = subject.UserId,
                    Bounds = subject.Bounds,
                };

                Logger.Debug($"Adding PhotoSubject (userid {subject.UserId}) to db", LogArea.Photos);

                this.database.PhotoSubjects.Add(subjectEntity);
            }
        }

        await this.database.SaveChangesAsync();

        string username = await this.database.UsernameFromGameToken(token);

        await WebhookHelper.SendWebhook
        (
            new EmbedBuilder
            {
                Title = "New photo uploaded!",
                Description = $"{username} uploaded a new photo.",
                ImageUrl = $"{ServerConfiguration.Instance.ExternalUrl}/gameAssets/{photo.LargeHash}",
                Color = WebhookHelper.GetEmbedColor(),
            }
        );

        return this.Ok();
    }

    [HttpGet("photos/{slotType}/{id:int}")]
    public async Task<IActionResult> SlotPhotos(string slotType, int id, [FromQuery] string? by)
    {

        if (SlotHelper.IsTypeInvalid(slotType)) return this.BadRequest();

        if (slotType == "developer") id = await SlotHelper.GetPlaceholderSlotId(this.database, id, SlotType.Developer);

        PaginationData pageData = this.Request.GetPaginationData();

        int creatorId = 0;
        if (by != null)
        {
            creatorId = await this.database.Users.Where(u => u.Username == by)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();
        }

        List<GamePhoto> photos = (await this.database.Photos.Include(p => p.PhotoSubjects)
            .Where(p => creatorId == 0 || p.CreatorId == creatorId)
            .Where(p => p.SlotId == id)
            .OrderByDescending(s => s.Timestamp)
            .ApplyPagination(pageData)
            .ToListAsync()).ToSerializableList(GamePhoto.CreateFromEntity);

        return this.Ok(new PhotoListResponse(photos));
    }

    [HttpGet("photos/by")]
    public async Task<IActionResult> UserPhotosBy(string user)
    {

        int targetUserId = await this.database.UserIdFromUsername(user);
        if (targetUserId == 0) return this.NotFound();

        PaginationData pageData = this.Request.GetPaginationData();

        List<GamePhoto> photos = (await this.database.Photos.Include(p => p.PhotoSubjects)
            .Where(p => p.CreatorId == targetUserId)
            .OrderByDescending(s => s.Timestamp)
            .ApplyPagination(pageData)
            .ToListAsync()).ToSerializableList(GamePhoto.CreateFromEntity);
        return this.Ok(new PhotoListResponse(photos));
    }

    [HttpGet("photos/with")]
    public async Task<IActionResult> UserPhotosWith(string user)
    {
        int targetUserId = await this.database.UserIdFromUsername(user);
        if (targetUserId == 0) return this.NotFound();

        PaginationData pageData = this.Request.GetPaginationData();

        List<GamePhoto> photos = (await this.database.Photos.Include(p => p.PhotoSubjects)
            .Where(p => p.PhotoSubjects.Any(ps => ps.UserId == targetUserId))
            .OrderByDescending(s => s.Timestamp)
            .ApplyPagination(pageData)
            .ToListAsync()).ToSerializableList(GamePhoto.CreateFromEntity);

        return this.Ok(new PhotoListResponse(photos));
    }

    [HttpPost("deletePhoto/{id:int}")]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        GameTokenEntity token = this.GetToken();

        PhotoEntity? photo = await this.database.Photos.FirstOrDefaultAsync(p => p.PhotoId == id);
        if (photo == null) return this.NotFound();

        // If user isn't photo creator then check if they own the level
        if (photo.CreatorId != token.UserId)
        {
            SlotEntity? photoSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == photo.SlotId && s.Type == SlotType.User);
            if (photoSlot == null || photoSlot.CreatorId != token.UserId) return this.Unauthorized();
        }

        this.database.Photos.Remove(photo);
        await this.database.SaveChangesAsync();
        return this.Ok();
    }
}