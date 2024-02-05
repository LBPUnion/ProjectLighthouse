﻿using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers.Moderator;

[ApiController]
[Route("moderation")]
public class ModerationRemovalController : ControllerBase
{
    private readonly DatabaseContext database;

    public ModerationRemovalController(DatabaseContext database)
    {
        this.database = database;
    }

    private async Task<IActionResult> Delete<T>(DbSet<T> dbSet, int id, string? callbackUrl, Func<UserEntity, int, Task<T?>> getHandler) where T: class
    {
        UserEntity? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        T? item = await getHandler(user, id);
        if (item == null) return this.Redirect("~/404");

        dbSet.Remove(item);
        await this.database.SaveChangesAsync();

        return this.Redirect(callbackUrl ?? "~/");
    }

    [HttpGet("deleteScore/{scoreId:int}")]
    public async Task<IActionResult> DeleteScore(int scoreId, [FromQuery] string? callbackUrl)
    {
        return await this.Delete<ScoreEntity>(this.database.Scores, scoreId, callbackUrl, async (user, id) =>
        {
            ScoreEntity? score = await this.database.Scores.Include(s => s.Slot).FirstOrDefaultAsync(s => s.ScoreId == id);
            if (score == null || !user.IsModerator) return null;

            if (score.Slot != null)
            {
                await this.database.SendNotification(score.UserId,
                    $"Your score on {score.Slot.Name} has been removed by a moderator.");
            }

            return score;
        });
    }

    [HttpGet("deleteComment/{commentId:int}")]
    public async Task<IActionResult> DeleteComment(int commentId, [FromQuery] string? callbackUrl)
    {
        UserEntity? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        CommentEntity? comment = await this.database.Comments
            .Include(c => c.TargetUser)
            .Include(c => c.TargetSlot)
            .FirstOrDefaultAsync(c => c.CommentId == commentId);
        if (comment == null) return this.Redirect("~/404");

        if (comment.Deleted) return this.Redirect(callbackUrl ?? "~/");

        bool canDelete;
        switch (comment.Type)
        {
            case CommentType.Level:
                int slotCreatorId = await this.database.Slots.Where(s => s.SlotId == comment.TargetSlotId)
                    .Select(s => s.CreatorId)
                    .FirstOrDefaultAsync();
                canDelete = user.UserId == comment.PosterUserId || user.UserId == slotCreatorId;
                break;
            case CommentType.Profile:
                canDelete = user.UserId == comment.PosterUserId || user.UserId == comment.TargetUserId;
                break;
            default: throw new ArgumentOutOfRangeException(nameof(commentId));
        }

        if (!canDelete && !user.IsModerator) return this.Redirect(callbackUrl ?? "~/");

        comment.Deleted = true;
        comment.DeletedBy = user.Username;
        comment.DeletedType = !canDelete && user.IsModerator ? "moderator" : "user";

        switch (comment.Type)
        {
            case CommentType.Profile when comment.DeletedType == "moderator" && comment.TargetUser != null:
            {
                await this.database.SendNotification(comment.PosterUserId,
                    $"Your comment on {comment.TargetUser.Username}'s profile has been removed by a moderator.");

                break;
            }
            case CommentType.Level when comment.DeletedType == "moderator" && comment.TargetSlot != null:
            {
                await this.database.SendNotification(comment.PosterUserId,
                    $"Your comment on level {comment.TargetSlot.Name} has been removed by a moderator.");

                break;
            }
            default: throw new ArgumentOutOfRangeException(nameof(comment.Type), @"Comment type is out of range.");
        }

        await this.database.SaveChangesAsync();

        return this.Redirect(callbackUrl ?? "~/");
    }

    [HttpGet("deleteReview/{reviewId:int}")]
    public async Task<IActionResult> DeleteReview(int reviewId, [FromQuery] string? callbackUrl)
    {
        UserEntity? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        ReviewEntity? review = await this.database.Reviews.Include(r => r.Slot).FirstOrDefaultAsync(c => c.ReviewId == reviewId);
        if (review == null) return this.Redirect("~/404");

        if (review.Deleted) return this.Redirect(callbackUrl ?? "~/");

        bool canDelete = review.Slot?.CreatorId == user.UserId;
        if (!canDelete && !user.IsModerator) return this.Redirect(callbackUrl ?? "~/");

        review.Deleted = true;
        review.DeletedBy = !canDelete && user.IsModerator ? DeletedBy.Moderator : DeletedBy.LevelAuthor;

        if (review.Slot != null && review.DeletedBy == DeletedBy.Moderator)
        {
            await this.database.SendNotification(review.ReviewerId,
                $"Your review on level {review.Slot.Name} has been removed by a moderator.");
        }

        await this.database.SaveChangesAsync();

        return this.Redirect(callbackUrl ?? "~/");
    }

    [HttpGet("deletePhoto/{photoId:int}")]
    public async Task<IActionResult> DeletePhoto(int photoId, [FromQuery] string? callbackUrl)
    {
        return await this.Delete<PhotoEntity>(this.database.Photos, photoId, callbackUrl, async (user, id) =>
        {
            PhotoEntity? photo = await this.database.Photos.Include(p => p.Slot).FirstOrDefaultAsync(p => p.PhotoId == id);
            if (photo == null) return null;

            if (!user.IsModerator && photo.CreatorId != user.UserId) return null;

            if (photo.Slot != null && user.IsModerator && photo.CreatorId != user.UserId)
            {
                await this.database.SendNotification(photo.CreatorId,
                    $"Your photo on level {photo.Slot.Name} has been removed by a moderator.");
            }

            return photo;
        });
    }

}