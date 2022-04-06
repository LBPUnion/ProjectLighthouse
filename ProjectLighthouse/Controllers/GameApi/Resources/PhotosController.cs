#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Discord;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.GameApi.Resources;

[ApiController]
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
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        if (user.PhotosByMe >= ServerSettings.Instance.PhotosQuota) return this.BadRequest();

        this.Request.Body.Position = 0;
        string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

        XmlSerializer serializer = new(typeof(Photo));
        Photo? photo = (Photo?)serializer.Deserialize(new StringReader(bodyString));
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

        if (photo.Subjects.Count > 4) return this.BadRequest();

        if (photo.Timestamp > TimestampHelper.Timestamp) return this.BadRequest();

        foreach (PhotoSubject subject in photo.Subjects)
        {
            subject.User = await this.database.Users.FirstOrDefaultAsync(u => u.Username == subject.Username);

            if (subject.User == null) continue;

            subject.UserId = subject.User.UserId;
            Logger.Log($"Adding PhotoSubject (userid {subject.UserId}) to db", LoggerLevelPhotos.Instance);

            this.database.PhotoSubjects.Add(subject);
        }

        await this.database.SaveChangesAsync();

        // Check for duplicate photo subjects
        List<int> subjectUserIds = new(4);
        foreach (PhotoSubject subject in photo.Subjects)
        {
            if (subjectUserIds.Contains(subject.UserId)) return this.BadRequest();

            subjectUserIds.Add(subject.UserId);
        }

        photo.PhotoSubjectIds = photo.Subjects.Select(subject => subject.PhotoSubjectId.ToString()).ToArray();

        //            photo.Slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == photo.SlotId);

        Logger.Log($"Adding PhotoSubjectCollection ({photo.PhotoSubjectCollection}) to photo", LoggerLevelPhotos.Instance);

        this.database.Photos.Add(photo);

        await this.database.SaveChangesAsync();

        await WebhookHelper.SendWebhook
        (
            new EmbedBuilder
            {
                Title = "New photo uploaded!",
                Description = $"{user.Username} uploaded a new photo.",
                ImageUrl = $"{ServerSettings.Instance.ExternalUrl}/gameAssets/{photo.LargeHash}",
                Color = WebhookHelper.UnionColor,
            }
        );

        return this.Ok();
    }

    [HttpGet("photos/user/{id:int}")]
    public async Task<IActionResult> SlotPhotos(int id)
    {
        List<Photo> photos = await this.database.Photos.Include(p => p.Creator).Take(10).ToListAsync();
        string response = photos.Aggregate(string.Empty, (s, photo) => s + photo.Serialize(id));
        return this.Ok(LbpSerializer.StringElement("photos", response));
    }

    [HttpGet("photos/by")]
    public async Task<IActionResult> UserPhotosBy([FromQuery] string user, [FromQuery] int pageStart, [FromQuery] int pageSize)
    {
        User? userFromQuery = await this.database.Users.FirstOrDefaultAsync(u => u.Username == user);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (userFromQuery == null) return this.NotFound();

        List<Photo> photos = await this.database.Photos.Include
                (p => p.Creator)
            .Where(p => p.CreatorId == userFromQuery.UserId)
            .OrderByDescending(s => s.Timestamp)
            .Skip(pageStart - 1)
            .Take(Math.Min(pageSize, 30))
            .ToListAsync();
        string response = photos.Aggregate(string.Empty, (s, photo) => s + photo.Serialize(0));
        return this.Ok(LbpSerializer.StringElement("photos", response));
    }

    [HttpGet("photos/with")]
    public async Task<IActionResult> UserPhotosWith([FromQuery] string user, [FromQuery] int pageStart, [FromQuery] int pageSize)
    {
        User? userFromQuery = await this.database.Users.FirstOrDefaultAsync(u => u.Username == user);
        // ReSharper disable once ConditionIsAlwaysTrueOrFalse
        if (userFromQuery == null) return this.NotFound();

        List<Photo> photos = new();
        foreach (Photo photo in this.database.Photos.Include
                     (p => p.Creator)) photos.AddRange(photo.Subjects.Where(subject => subject.User.UserId == userFromQuery.UserId).Select(_ => photo));

        string response = photos.OrderByDescending
                (s => s.Timestamp)
            .Skip(pageStart - 1)
            .Take(Math.Min(pageSize, 30))
            .Aggregate(string.Empty, (s, photo) => s + photo.Serialize(0));

        return this.Ok(LbpSerializer.StringElement("photos", response));
    }

    [HttpPost("deletePhoto/{id:int}")]
    public async Task<IActionResult> DeletePhoto(int id)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        Photo? photo = await this.database.Photos.FirstOrDefaultAsync(p => p.PhotoId == id);
        if (photo == null) return this.NotFound();
        if (photo.CreatorId != user.UserId) return this.StatusCode(401, "");

        this.database.Photos.Remove(photo);
        await this.database.SaveChangesAsync();
        return this.Ok();
    }
}