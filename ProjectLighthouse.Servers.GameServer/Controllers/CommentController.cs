#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Filter;
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
public class CommentController : ControllerBase
{
    private readonly DatabaseContext database;
    public CommentController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpPost("rateUserComment/{username}")]
    [HttpPost("rateComment/{slotType}/{slotId:int}")]
    public async Task<IActionResult> RateComment([FromQuery] int commentId, [FromQuery] int rating, string? username, string? slotType, int slotId)
    {
        GameTokenEntity token = this.GetToken();

        // Return bad request if both are true or both are false
        if ((slotId == 0 || SlotHelper.IsTypeInvalid(slotType)) == (username == null)) return this.BadRequest();

        bool success = await this.database.RateComment(token.UserId, commentId, rating);
        if (!success) return this.BadRequest();

        return this.Ok();
    }

    [HttpGet("comments/{slotType}/{slotId:int}")]
    [HttpGet("userComments/{username}")]
    public async Task<IActionResult> GetComments(string? username, string? slotType, int slotId)
    {
        GameTokenEntity token = this.GetToken();

        UserEntity? user = await this.database.UserFromGameToken(token);
        if (user == null) return this.Unauthorized();

        if ((slotId == 0 || SlotHelper.IsTypeInvalid(slotType)) == (username == null)) return this.BadRequest();

        int originalSlotId = slotId;

        if (slotType == "developer") slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

        int targetId;
        CommentType type = username == null ? CommentType.Level : CommentType.Profile;

        PaginationData pageData = this.Request.GetPaginationData();

        IQueryable<CommentEntity> baseQuery = this.database.Comments.Where(c => c.Type == type);

        if (type == CommentType.Level)
        {
            targetId = await this.database.Slots.Where(s => s.SlotId == slotId)
                .Where(s => s.CommentsEnabled && !s.Hidden)
                .Select(s => s.SlotId)
                .FirstOrDefaultAsync();

            baseQuery = baseQuery.Where(c => c.TargetSlotId == targetId);
        }
        else
        {
            targetId = await this.database.Users.Where(u => u.Username == username)
                .Where(u => u.CommentsEnabled)
                .Select(u => u.UserId)
                .FirstOrDefaultAsync();

            baseQuery = baseQuery.Where(c => c.TargetUserId == targetId);
        }

        if (targetId == 0) return this.NotFound();

        List<int> blockedUsers =  await (
                from blockedProfile in this.database.BlockedProfiles
                where blockedProfile.UserId == token.UserId
                select blockedProfile.BlockedUserId).ToListAsync();

        List<GameComment> comments = (await baseQuery.OrderByDescending(c => c.Timestamp)
            .Where(c => !blockedUsers.Contains(c.PosterUserId))
            .Include(c => c.Poster)
            .Where(c => c.Poster.PermissionLevel != PermissionLevel.Banned)
            .ApplyPagination(pageData)
            .ToListAsync()).ToSerializableList(c => GameComment.CreateFromEntity(c, token.UserId));

        if (type == CommentType.Level && slotType == "developer" && user.IsModerator && pageData.PageStart == 1)
        {
            comments.Insert(0, new GameComment
            {
                CommentId = 0,
                Timestamp = 0,
                AuthorUsername = "LH",
                Message = $"Slot ID: {targetId}, Story level ID: {originalSlotId}",
            });
        }

        return this.Ok(new CommentListResponse(comments));
    }

    [HttpPost("postUserComment/{username}")]
    [HttpPost("postComment/{slotType}/{slotId:int}")]
    public async Task<IActionResult> PostComment(string? username, string? slotType, int slotId)
    {
        GameTokenEntity token = this.GetToken();

        // Deny request if in read-only mode
        if (ServerConfiguration.Instance.UserGeneratedContentLimits.ReadOnlyMode) return this.BadRequest();

        GameComment? comment = await this.DeserializeBody<GameComment>();
        if (comment?.Message == null) return this.BadRequest();

        if ((slotId == 0 || SlotHelper.IsTypeInvalid(slotType)) == (username == null)) return this.BadRequest();

        if (slotType == "developer") slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

        CommentType type = username == null ? CommentType.Level : CommentType.Profile;

        int targetId;
        if (type == CommentType.Level)
        {
            targetId = await this.database.Slots.Where(s => s.SlotId == slotId)
                .Where(s => s.CommentsEnabled && !s.Hidden)
                .Select(s => s.SlotId)
                .FirstOrDefaultAsync();
        }
        else
        {
            targetId = await this.database.UserIdFromUsername(username!);
        }

        string filteredText = CensorHelper.FilterMessage(comment.Message);

        if (ServerConfiguration.Instance.LogChatFiltering && filteredText != comment.Message)
            Logger.Info($"Censored profane word(s) from in-game comment sent by {username}: \"{comment.Message}\" => \"{filteredText}\"",
                LogArea.Filter);

        bool success = await this.database.PostComment(token.UserId, targetId, type, filteredText);
        if (success) return this.Ok();

        return this.BadRequest();
    }

    [HttpPost("deleteUserComment/{username}")]
    [HttpPost("deleteComment/{slotType}/{slotId:int}")]
    public async Task<IActionResult> DeleteComment([FromQuery] int commentId, string? username, string? slotType, int slotId)
    {
        GameTokenEntity token = this.GetToken();

        // Deny request if in read-only mode
        if (ServerConfiguration.Instance.UserGeneratedContentLimits.ReadOnlyMode) return this.BadRequest();

        if ((slotId == 0 || SlotHelper.IsTypeInvalid(slotType)) == (username == null)) return this.BadRequest();

        CommentEntity? comment = await this.database.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
        if (comment == null) return this.NotFound();

        if (comment.Deleted) return this.Ok();

        bool canDelete;
        if (comment.Type == CommentType.Profile)
        {
            canDelete = comment.PosterUserId == token.UserId || comment.TargetUserId == token.UserId;
        }
        else
        {
            if (slotType == "developer") slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

            if (slotId != comment.TargetSlotId) return this.BadRequest();

            int slotCreator = await this.database.Slots.Where(s => s.SlotId == comment.TargetSlotId)
                .Where(s => s.CommentsEnabled)
                .Select(s => s.CreatorId)
                .FirstOrDefaultAsync();

            // Comments are disabled or the slot doesn't have a creator
            if (slotCreator == 0) return this.BadRequest();

            canDelete = comment.PosterUserId == token.UserId || slotCreator == token.UserId;
        }

        if (!canDelete) return this.Forbid();

        comment.Deleted = true;
        comment.DeletedBy = await this.database.UsernameFromGameToken(token);
        comment.DeletedType = "user";

        await this.database.SaveChangesAsync();
        return this.Ok();
    }
}