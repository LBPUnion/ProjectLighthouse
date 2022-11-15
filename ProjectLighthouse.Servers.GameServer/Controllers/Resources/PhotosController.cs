#nullable enable
using Discord;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
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
    private readonly Database database;

    public PhotosController(Database database)
    {
        this.database = database;
    }

    [HttpPost("uploadPhoto")]
    public async Task<IActionResult> UploadPhoto()
    {
        User? user = await this.database.UserFromGameToken(this.GetToken());
        if (user == null) return this.StatusCode(403, "");

        if (user.PhotosByMe >= ServerConfiguration.Instance.UserGeneratedContentLimits.PhotosQuota) return this.BadRequest();

        Photo? photo = await this.DeserializeBody<Photo>();
        if (photo == null) return this.BadRequest();

        SanitizationHelper.SanitizeStringsInClass(photo);

        foreach (Photo p in this.database.Photos.Where(p => p.CreatorId == user.UserId))
        {
            if (p.LargeHash == photo.LargeHash) return this.Ok(); // photo already uplaoded
            if (p.MediumHash == photo.MediumHash) return this.Ok();
            if (p.SmallHash == photo.SmallHash) return this.Ok();
            if (p.PlanHash == photo.PlanHash) return this.Ok();
        }

        photo.CreatorId = user.UserId;
        photo.Creator = user;

        if (photo.XmlLevelInfo != null)
        {
            bool validLevel = false;
            PhotoSlot photoSlot = photo.XmlLevelInfo;
            if (photoSlot.SlotType is SlotType.Pod or SlotType.Local) photoSlot.SlotId = 0;
            switch (photoSlot.SlotType)
            {
                case SlotType.User:
                {
                    // We'll grab the slot by the RootLevel and see what happens from here.
                    Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.Type == SlotType.User && s.ResourceCollection.Contains(photoSlot.RootLevel));
                    if (slot == null) break;

                    if (!string.IsNullOrEmpty(slot.RootLevel)) validLevel = true;
                    if (slot.IsAdventurePlanet) photoSlot.SlotId = slot.SlotId;
                    break;
                }
                case SlotType.Pod:
                case SlotType.Local:
                case SlotType.Developer:
                {
                    Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.Type == photoSlot.SlotType && s.InternalSlotId == photoSlot.SlotId);
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

            if (validLevel) photo.SlotId = photo.XmlLevelInfo.SlotId;
        }

        if (photo.Subjects.Count > 4) return this.BadRequest();

        if (photo.Timestamp > TimeHelper.Timestamp) photo.Timestamp = TimeHelper.Timestamp;

        // Check for duplicate photo subjects
        List<string> subjectUserIds = new(4);
        foreach (PhotoSubject subject in photo.Subjects)
        {
            if (subjectUserIds.Contains(subject.Username) && !string.IsNullOrEmpty(subject.Username)) return this.BadRequest();

            subjectUserIds.Add(subject.Username);
        }

        foreach (PhotoSubject subject in photo.Subjects.Where(subject => !string.IsNullOrEmpty(subject.Username)))
        {
            subject.User = await this.database.Users.FirstOrDefaultAsync(u => u.Username == subject.Username);

            if (subject.User == null) continue;

            subject.UserId = subject.User.UserId;
            Logger.Debug($"Adding PhotoSubject (userid {subject.UserId}) to db", LogArea.Photos);

            this.database.PhotoSubjects.Add(subject);
        }

        await this.database.SaveChangesAsync();

        photo.PhotoSubjectIds = photo.Subjects.Where(s => s.UserId != 0).Select(subject => subject.PhotoSubjectId.ToString()).ToArray();

        Logger.Debug($"Adding PhotoSubjectCollection ({photo.PhotoSubjectCollection}) to photo", LogArea.Photos);

        this.database.Photos.Add(photo);

        await this.database.SaveChangesAsync();

        await WebhookHelper.SendWebhook
        (
            new EmbedBuilder
            {
                Title = "New photo uploaded!",
                Description = $"{user.Username} uploaded a new photo.",
                ImageUrl = $"{ServerConfiguration.Instance.ExternalUrl}/gameAssets/{photo.LargeHash}",
                Color = WebhookHelper.UnionColor,
            }
        );

        return this.Ok();
    }

    [HttpGet("photos/{slotType}/{id:int}")]
    public async Task<IActionResult> SlotPhotos([FromQuery] int pageStart, [FromQuery] int pageSize, string slotType, int id)
    {
        if (pageSize <= 0) return this.BadRequest();

        if (SlotHelper.IsTypeInvalid(slotType)) return this.BadRequest();

        if (slotType == "developer") id = await SlotHelper.GetPlaceholderSlotId(this.database, id, SlotType.Developer);

        List<Photo> photos = await this.database.Photos.Include(p => p.Creator)
            .Where(p => p.SlotId == id)
            .OrderByDescending(s => s.Timestamp)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30))
            .ToListAsync();
        string response = photos.Aggregate(string.Empty, (s, photo) => s + photo.Serialize(id, SlotHelper.ParseType(slotType)));
        return this.Ok(LbpSerializer.StringElement("photos", response));
    }

    [HttpGet("photos/by")]
    public async Task<IActionResult> UserPhotosBy([FromQuery] string user, [FromQuery] int pageStart, [FromQuery] int pageSize)
    {
        if (pageSize <= 0) return this.BadRequest();

        int targetUserId = await this.database.UserIdFromUsername(user);
        if (targetUserId == 0) return this.NotFound();

        List<Photo> photos = await this.database.Photos.Include
                (p => p.Creator)
            .Where(p => p.CreatorId == targetUserId)
            .OrderByDescending(s => s.Timestamp)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30))
            .ToListAsync();
        string response = photos.Aggregate(string.Empty, (s, photo) => s + photo.Serialize());
        return this.Ok(LbpSerializer.StringElement("photos", response));
    }

    [HttpGet("photos/with")]
    public async Task<IActionResult> UserPhotosWith([FromQuery] string user, [FromQuery] int pageStart, [FromQuery] int pageSize)
    {
        if (pageSize <= 0) return this.BadRequest();

        int targetUserId = await this.database.UserIdFromUsername(user);
        if (targetUserId == 0) return this.NotFound();

        List<int> photoSubjectIds = new();
        photoSubjectIds.AddRange(this.database.PhotoSubjects.Where(p => p.UserId == targetUserId).Select(p => p.PhotoSubjectId));

        var list = this.database.Photos.Select(p => new
        {
            p.PhotoId,
            p.PhotoSubjectCollection,
        }).ToList();
        List<int> photoIds = (from v in list where photoSubjectIds.Any(ps => v.PhotoSubjectCollection.Split(",").Contains(ps.ToString())) select v.PhotoId).ToList();

        string response = Enumerable.Aggregate(
            this.database.Photos.Where(p => photoIds.Any(id => p.PhotoId == id) && p.CreatorId != targetUserId)
                .OrderByDescending(s => s.Timestamp)
                .Skip(Math.Max(0, pageStart - 1))
                .Take(Math.Min(pageSize, 30)),
            string.Empty,
            (current, photo) => current + photo.Serialize());

        return this.Ok(LbpSerializer.StringElement("photos", response));
    }

    [HttpPost("deletePhoto/{id:int}")]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        GameToken token = this.GetToken();

        Photo? photo = await this.database.Photos.FirstOrDefaultAsync(p => p.PhotoId == id);
        if (photo == null) return this.NotFound();

        // If user isn't photo creator then check if they own the level
        if (photo.CreatorId != token.UserId)
        {
            Slot? photoSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == photo.SlotId && s.Type == SlotType.User);
            if (photoSlot == null || photoSlot.CreatorId != token.UserId) return this.StatusCode(401, "");
        }
        foreach (string idStr in photo.PhotoSubjectIds)
        {
            if (string.IsNullOrWhiteSpace(idStr)) continue;

            if (!int.TryParse(idStr, out int subjectId)) continue;

            this.database.PhotoSubjects.RemoveWhere(p => p.PhotoSubjectId == subjectId);
        }

        HashSet<string> photoResources = new(){photo.LargeHash, photo.SmallHash, photo.MediumHash, photo.PlanHash,};
        foreach (string hash in photoResources)
        {
            if (System.IO.File.Exists(Path.Combine("png", $"{hash}.png")))
            {
                System.IO.File.Delete(Path.Combine("png", $"{hash}.png"));
            }
            if (System.IO.File.Exists(Path.Combine("r", hash)))
            {
                System.IO.File.Delete(Path.Combine("r", hash));
            }
        }

        this.database.Photos.Remove(photo);
        await this.database.SaveChangesAsync();
        return this.Ok();
    }
}