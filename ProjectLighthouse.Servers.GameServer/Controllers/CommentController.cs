#nullable enable
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

[ApiController]
[Authorize]
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
    [HttpPost("rateComment/{slotType}/{slotId:int}")]
    public async Task<IActionResult> RateComment([FromQuery] int commentId, [FromQuery] int rating, string? username, string? slotType, int slotId)
    {
        GameToken token = this.GetToken();

        // Return bad request if both are true or both are false
        if ((slotId == 0 || SlotHelper.IsTypeInvalid(slotType)) == (username == null)) return this.BadRequest();

        bool success = await this.database.RateComment(token.UserId, commentId, rating);
        if (!success) return this.BadRequest();

        return this.Ok();
    }

    [HttpGet("comments/{slotType}/{slotId:int}")]
    [HttpGet("userComments/{username}")]
    public async Task<IActionResult> GetComments([FromQuery] int pageStart, [FromQuery] int pageSize, string? username, string? slotType, int slotId)
    {
        GameToken token = this.GetToken();

        if (pageSize <= 0 || pageStart < 0) return this.BadRequest();

        if ((slotId == 0 || SlotHelper.IsTypeInvalid(slotType)) == (username == null)) return this.BadRequest();

        if (slotType == "developer") slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

        int targetId;
        CommentType type = username == null ? CommentType.Level : CommentType.Profile;

        if (type == CommentType.Level)
        {
            targetId = await this.database.Slots.Where(s => s.SlotId == slotId)
                .Where(s => s.CommentsEnabled && !s.Hidden)
                .Select(s => s.SlotId)
                .FirstOrDefaultAsync();
        }
        else
        {
            targetId = await this.database.Users.Where(u => u.Username == username)
                .Where(u => u.CommentsEnabled)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();
        }

        if (targetId == 0) return this.NotFound();

        List<Comment> comments = await this.database.Comments.Include
                (c => c.Poster)
            .Where(c => c.TargetId == targetId && c.Type == type)
            .OrderByDescending(c => c.Timestamp)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(Math.Min(pageSize, 30))
            .ToListAsync();

        string outputXml = comments.Aggregate
            (string.Empty, (current, comment) => current + comment.Serialize(this.getReaction(token.UserId, comment.CommentId).Result));
        return this.Ok(LbpSerializer.StringElement("comments", outputXml));
    }

    private async Task<int> getReaction(int userId, int commentId)
    {
        return await this.database.Reactions.Where(r => r.UserId == userId)
            .Where(r => r.TargetId == commentId)
            .Select(r => r.Rating)
            .FirstOrDefaultAsync();
    }

    [HttpPost("postUserComment/{username}")]
    [HttpPost("postComment/{slotType}/{slotId:int}")]
    public async Task<IActionResult> PostComment(string? username, string? slotType, int slotId)
    {
        GameToken token = this.GetToken();

        Comment? comment = await this.DeserializeBody<Comment>();
        if (comment == null) return this.BadRequest();

        if ((slotId == 0 || SlotHelper.IsTypeInvalid(slotType)) == (username == null)) return this.BadRequest();

        CommentType type = username == null ? CommentType.Level : CommentType.Profile;

        int targetId;
        if (type == CommentType.Level)
        {
            slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);
            targetId = await this.database.Slots.Where(s => s.SlotId == slotId)
                .Where(s => s.CommentsEnabled && !s.Hidden)
                .Select(s => s.SlotId)
                .FirstOrDefaultAsync();
        }
        else
        {
            targetId = await this.database.UserIdFromUsername(username!);
        }

        bool success = await this.database.PostComment(token.UserId, targetId, type, comment.Message);
        if (success) return this.Ok();

        return this.BadRequest();
    }

    [HttpPost("deleteUserComment/{username}")]
    [HttpPost("deleteComment/{slotType}/{slotId:int}")]
    public async Task<IActionResult> DeleteComment([FromQuery] int commentId, string? username, string? slotType, int slotId)
    {
        GameToken token = this.GetToken();

        if ((slotId == 0 || SlotHelper.IsTypeInvalid(slotType)) == (username == null)) return this.BadRequest();

        Comment? comment = await this.database.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
        if (comment == null) return this.NotFound();

        if (comment.Deleted) return this.Ok();

        bool canDelete;
        if (comment.Type == CommentType.Profile)
        {
            canDelete = comment.PosterUserId == token.UserId || comment.TargetId == token.UserId;
        }
        else
        {
            if (slotType == "developer") slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

            if (slotId != comment.TargetId) return this.BadRequest();

            int slotCreator = await this.database.Slots.Where(s => s.SlotId == comment.TargetId)
                .Where(s => s.CommentsEnabled)
                .Select(s => s.CreatorId)
                .FirstOrDefaultAsync();

            // Comments are disabled or the slot doesn't have a creator
            if (slotCreator == 0) return this.BadRequest();

            canDelete = comment.PosterUserId == token.UserId || slotCreator == token.UserId;
        }

        if (!canDelete) return this.StatusCode(403, "");

        comment.Deleted = true;
        comment.DeletedBy = await this.database.UsernameFromGameToken(token);
        comment.DeletedType = "user";

        await this.database.SaveChangesAsync();
        return this.Ok();
    }
}