﻿using LBPUnion.ProjectLighthouse.Administration;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.PlayerData.Reviews;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers.Moderator;

[ApiController]
[Route("moderation")]
public class ModerationRemovalController : ControllerBase
{

    private readonly Database database;

    public ModerationRemovalController(Database database)
    {
        this.database = database;
    }

    private async Task<IActionResult> Delete<T>(DbSet<T> dbSet, int id, string? callbackUrl, Func<User, int, Task<T?>> getHandler) where T: class
    {
        User? user = this.database.UserFromWebRequest(this.Request);
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
        return await this.Delete<Score>(this.database.Scores, scoreId, callbackUrl, async (user, id) =>
        {
            Score? score = await this.database.Scores.Include(s => s.Slot).FirstOrDefaultAsync(s => s.ScoreId == id);
            if (score == null) return null;

            if (!user.IsModerator && score.Slot.CreatorId != user.UserId) return null;

            return score;
        });
    }

    [HttpGet("deleteComment/{commentId:int}")]
    public async Task<IActionResult> DeleteComment(int commentId, [FromQuery] string? callbackUrl)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        Comment? comment = await this.database.Comments.FirstOrDefaultAsync(c => c.CommentId == commentId);
        if (comment == null) return this.Redirect("~/404");

        if (comment.Deleted) return this.Redirect(callbackUrl ?? "~/");

        bool canDelete;
        switch (comment.Type)
        {
            case CommentType.Level:
                int slotCreatorId = await this.database.Slots.Where(s => s.SlotId == comment.TargetId)
                    .Select(s => s.CreatorId)
                    .FirstOrDefaultAsync();
                canDelete = user.UserId == comment.PosterUserId || user.UserId == slotCreatorId;
                break;
            case CommentType.Profile:
                canDelete = user.UserId == comment.PosterUserId || user.UserId == comment.TargetId;
                break;
            default: throw new ArgumentOutOfRangeException();
        }

        if (!canDelete && !user.IsModerator) return this.Redirect(callbackUrl ?? "~/");

        comment.Deleted = true;
        comment.DeletedBy = user.Username;
        comment.DeletedType = !canDelete && user.IsModerator ? "moderator" : "user";
        await this.database.SaveChangesAsync();

        return this.Redirect(callbackUrl ?? "~/");
    }

    [HttpGet("deleteReview/{reviewId:int}")]
    public async Task<IActionResult> DeleteReview(int reviewId, [FromQuery] string? callbackUrl)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        Review? review = await this.database.Reviews.Include(r => r.Slot).FirstOrDefaultAsync(c => c.ReviewId == reviewId);
        if (review == null) return this.Redirect("~/404");

        if (review.Deleted) return this.Redirect(callbackUrl ?? "~/");

        bool canDelete = review.Slot?.CreatorId == user.UserId;
        if (!canDelete && !user.IsModerator) return this.Redirect(callbackUrl ?? "~/");

        review.Deleted = true;
        review.DeletedBy = !canDelete && user.IsModerator ? DeletedBy.Moderator : DeletedBy.LevelAuthor;
        await this.database.SaveChangesAsync();

        return this.Redirect(callbackUrl ?? "~/");
    }

    [HttpGet("deletePhoto/{photoId:int}")]
    public async Task<IActionResult> DeletePhoto(int photoId, [FromQuery] string? callbackUrl)
    {
        return await this.Delete<Photo>(this.database.Photos, photoId, callbackUrl, async (user, id) =>
        {
            Photo? photo = await this.database.Photos.Include(p => p.Slot).FirstOrDefaultAsync(p => p.PhotoId == id);
            if (photo == null) return null;

            if (!user.IsModerator && photo.Slot?.CreatorId != user.UserId) return null;

            return photo;
        });
    }

}