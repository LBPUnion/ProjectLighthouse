#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.GameApi;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class CommentController : ControllerBase
{
    private readonly Database database;
    public CommentController(Database database)
    {
        this.database = database;
    }

    [HttpPost("rateUserComment/{username}")]
    [HttpPost("rateComment/user/{slotId:int}")]
    public async Task<IActionResult> RateComment([FromQuery] int commentId, [FromQuery] int rating, string? username, int? slotId)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        Comment? comment = await this.database.Comments.Include(c => c.Poster).FirstOrDefaultAsync(c => commentId == c.CommentId);

        if (comment == null) return this.BadRequest();

        Reaction? reaction = await this.database.Reactions.FirstOrDefaultAsync(r => r.UserId == user.UserId && r.TargetId == commentId);
        if (reaction == null)
        {
            Reaction newReaction = new Reaction()
            {
                UserId = user.UserId,
                TargetId = commentId,
                Rating = 0,
            };
            this.database.Reactions.Add(newReaction);
            await this.database.SaveChangesAsync();
            reaction = newReaction;
        }
        int oldRating = reaction.Rating;
        if (oldRating == rating) return this.Ok();

        reaction.Rating = rating;
        // if rating changed then we count the number of reactions to ensure accuracy
        List<Reaction> reactions = await this.database.Reactions
        .Where(c => c.TargetId == commentId)
        .ToListAsync();
        int yay = 0;
        int boo = 0;
        foreach (Reaction r in reactions)
        {
            switch (r.Rating)
            {
                case -1:
                    boo++;
                    break;
                case 1: 
                    yay++;
                    break;
            }
        }

        comment.ThumbsDown = boo;
        comment.ThumbsUp = yay;
        await this.database.SaveChangesAsync();
        return this.Ok();
    }


    [HttpGet("comments/user/{slotId:int}")]
    [HttpGet("userComments/{username}")]
    public async Task<IActionResult> GetComments([FromQuery] int pageStart, [FromQuery] int pageSize, string? username, int? slotId)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");
        
        int targetId = slotId.GetValueOrDefault();
        CommentType type = CommentType.Level;
        if (!string.IsNullOrWhiteSpace(username))
        {
            targetId = this.database.Users.First(u => u.Username.Equals(username)).UserId;
            type = CommentType.Profile;
        }

        List<Comment> comments = await this.database.Comments
            .Include(c => c.Poster)
            .Where(c => c.TargetId == targetId && c.Type == type)
            .OrderByDescending(c => c.Timestamp)
            .Skip(pageStart - 1)
            .Take(Math.Min(pageSize,
                30))
            .ToListAsync();

        string outputXml = comments.Aggregate(string.Empty, (current, comment) => current +
                comment.Serialize(this.getReaction(user.UserId, comment.CommentId).Result));
        return this.Ok(LbpSerializer.StringElement("comments", outputXml));
    }

    public async Task<int> getReaction(int userId, int commentId)
    {
        Reaction? reaction = await this.database.Reactions.FirstOrDefaultAsync(r => r.UserId == userId && r.TargetId == commentId);
        if (reaction == null) return 0;
        return reaction.Rating;
    }

    [HttpPost("postUserComment/{username}")]
    [HttpPost("postComment/user/{slotId:int}")]
    public async Task<IActionResult> PostComment(string? username, int? slotId)
    {
        this.Request.Body.Position = 0;                                                                       
        string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

        XmlSerializer serializer = new(typeof(Comment));
        Comment? comment = (Comment?) serializer.Deserialize(new StringReader(bodyString));

        CommentType type = (slotId.GetValueOrDefault() == 0 ? CommentType.Profile : CommentType.Level);

        User? poster = await this.database.UserFromGameRequest(this.Request);
        if (poster == null) return this.StatusCode(403, "");

        if (comment == null) return this.BadRequest();

        int targetId = slotId.GetValueOrDefault();

        if (type == CommentType.Profile)
        {
            User? target = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
            if (target == null) return this.BadRequest();
            targetId = target.UserId;
        }
        else
        {
            Slot? target = await this.database.Slots.FirstOrDefaultAsync(u => u.SlotId == slotId);
            if (target == null) return this.BadRequest();
        }

        comment.PosterUserId = poster.UserId;
        comment.TargetId = targetId;
        comment.Type = type;

        comment.Timestamp = TimeHelper.UnixTimeMilliseconds();

        this.database.Comments.Add(comment);
        await this.database.SaveChangesAsync();
        return this.Ok();
    }

    [HttpPost("deleteUserComment/{username}")]
    [HttpPost("deleteComment/user/{slotId:int}")]
    public async Task<IActionResult> DeleteComment([FromQuery] int commentId, string? username, int? slotId)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        Comment? comment = await this.database.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
        if (comment == null) return this.NotFound();
        // if you are not the poster
        if (comment.PosterUserId != user.UserId)
        {
            if (comment.Type == CommentType.Profile)
            {
                // if you aren't the poster and aren't the profile owner
                if (comment.TargetId != user.UserId)
                {
                    return this.StatusCode(403, "");
                }
            }
            else
            {
                Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == comment.TargetId);
                // if you aren't the creator of the level
                if (slot == null || slot.CreatorId != user.UserId || slotId.GetValueOrDefault() != slot.SlotId)
                {
                    return this.StatusCode(403, "");
                }
            }
        }

        comment.Deleted = true;
        comment.DeletedBy = user.Username;
        comment.DeletedType = "user";

        await this.database.SaveChangesAsync();

        return this.Ok();
    }
}