#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class ReviewController : ControllerBase
{
    private readonly DatabaseContext database;

    public ReviewController(DatabaseContext database)
    {
        this.database = database;
    }

    // LBP1 rating
    [HttpPost("rate/user/{slotId:int}")]
    public async Task<IActionResult> Rate(int slotId, int rating)
    {
        GameTokenEntity token = this.GetToken();

        SlotEntity? slot = await this.database.Slots.Include(s => s.Creator).FirstOrDefaultAsync(s => s.SlotId == slotId);
        if (slot == null) return this.Forbid();

        RatedLevelEntity? ratedLevel = await this.database.RatedLevels.FirstOrDefaultAsync(r => r.SlotId == slotId && r.UserId == token.UserId);
        if (ratedLevel == null)
        {
            ratedLevel = new RatedLevelEntity
            {
                SlotId = slotId,
                UserId = token.UserId,
                Rating = 0,
                TagLBP1 = "",
            };
            this.database.RatedLevels.Add(ratedLevel);
        }

        ratedLevel.RatingLBP1 = Math.Clamp(rating, 0, 5);

        await this.database.SaveChangesAsync();

        return this.Ok();
    }

    // LBP2 and beyond rating
    [HttpPost("dpadrate/user/{slotId:int}")]
    public async Task<IActionResult> DPadRate(int slotId, int rating)
    {
        GameTokenEntity token = this.GetToken();

        SlotEntity? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slotId);
        if (slot == null) return this.Forbid();

        RatedLevelEntity? ratedLevel = await this.database.RatedLevels.FirstOrDefaultAsync(r => r.SlotId == slotId && r.UserId == token.UserId);
        if (ratedLevel == null)
        {
            ratedLevel = new RatedLevelEntity
            {
                SlotId = slotId,
                UserId = token.UserId,
                RatingLBP1 = 0,
                TagLBP1 = "",
            };
            this.database.RatedLevels.Add(ratedLevel);
        }

        ratedLevel.Rating = Math.Clamp(rating, -1, 1);

        ReviewEntity? review = await this.database.Reviews.FirstOrDefaultAsync(r => r.SlotId == slotId && r.ReviewerId == token.UserId);
        if (review != null) review.Thumb = ratedLevel.Rating;

        await this.database.SaveChangesAsync();

        return this.Ok();
    }

    [HttpPost("postReview/user/{slotId:int}")]
    public async Task<IActionResult> PostReview(int slotId)
    {
        GameTokenEntity token = this.GetToken();

        GameReview? newReview = await this.DeserializeBody<GameReview>();
        if (newReview == null) return this.BadRequest();

        newReview.Text = CensorHelper.FilterMessage(newReview.Text);

        if (newReview.Text.Length > 512) return this.BadRequest();

        ReviewEntity? review = await this.database.Reviews.FirstOrDefaultAsync(r => r.SlotId == slotId && r.ReviewerId == token.UserId);

        if (review == null)
        {
            review = new ReviewEntity
            {
                SlotId = slotId,
                ReviewerId = token.UserId,
                DeletedBy = DeletedBy.None,
                ThumbsUp = 0,
                ThumbsDown = 0,
            };
            this.database.Reviews.Add(review);
        }
        review.Thumb = Math.Clamp(newReview.Thumb, -1, 1);
        review.LabelCollection = LabelHelper.RemoveInvalidLabels(newReview.LabelCollection);
        
        review.Text = newReview.Text;
        review.Deleted = false;
        review.Timestamp = TimeHelper.TimestampMillis;

        // sometimes the game posts/updates a review rating without also calling dpadrate/user/etc (why??)
        RatedLevelEntity? ratedLevel = await this.database.RatedLevels.FirstOrDefaultAsync(r => r.SlotId == slotId && r.UserId == token.UserId);
        if (ratedLevel == null)
        {
            ratedLevel = new RatedLevelEntity
            {
                SlotId = slotId,
                UserId = token.UserId,
                RatingLBP1 = 0,
                TagLBP1 = "",
            };
            this.database.RatedLevels.Add(ratedLevel);
        }

        ratedLevel.Rating = Math.Clamp(newReview.Thumb, -1, 1);

        await this.database.SaveChangesAsync();

        return this.Ok();
    }

    [HttpGet("reviewsFor/user/{slotId:int}")]
    public async Task<IActionResult> ReviewsFor(int slotId)
    {
        GameTokenEntity token = this.GetToken();

        PaginationData pageData = this.Request.GetPaginationData();

        SlotEntity? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slotId);
        if (slot == null) return this.BadRequest();

        List<GameReview> reviews = (await this.database.Reviews
            .Where(r => r.SlotId == slotId)
            .OrderByDescending(r => r.ThumbsUp - r.ThumbsDown)
            .ThenByDescending(r => r.Timestamp)
            .ApplyPagination(pageData)
            .ToListAsync()).ToSerializableList(r => GameReview.CreateFromEntity(r, token));

        return this.Ok(new ReviewResponse(reviews, reviews.LastOrDefault()?.Timestamp ?? TimeHelper.TimestampMillis, pageData.HintStart));
    }

    [HttpGet("reviewsBy/{username}")]
    public async Task<IActionResult> ReviewsBy(string username)
    {
        GameTokenEntity token = this.GetToken();

        PaginationData pageData = this.Request.GetPaginationData();

        int targetUserId = await this.database.UserIdFromUsername(username);

        if (targetUserId == 0) return this.BadRequest();

        List<GameReview> reviews = (await this.database.Reviews
            .Where(r => r.ReviewerId == targetUserId)
            .OrderByDescending(r => r.Timestamp)
            .ApplyPagination(pageData)
            .ToListAsync()).ToSerializableList(r => GameReview.CreateFromEntity(r, token));

        return this.Ok(new ReviewResponse(reviews, reviews.LastOrDefault()?.Timestamp ?? TimeHelper.TimestampMillis, pageData.HintStart));
    }

    [HttpPost("rateReview/user/{slotId:int}/{username}")]
    public async Task<IActionResult> RateReview(int slotId, string username, int rating = 0)
    {
        GameTokenEntity token = this.GetToken();

        int reviewerId = await this.database.UserIdFromUsername(username);
        if (reviewerId == 0) return this.BadRequest();

        ReviewEntity? review = await this.database.Reviews.FirstOrDefaultAsync(r => r.SlotId == slotId && r.ReviewerId == reviewerId);
        if (review == null) return this.BadRequest();

        RatedReviewEntity? ratedReview = await this.database.RatedReviews.FirstOrDefaultAsync(r => r.ReviewId == review.ReviewId && r.UserId == token.UserId);
        if (ratedReview == null)
        {
            ratedReview = new RatedReviewEntity
            {
                ReviewId = review.ReviewId,
                UserId = token.UserId,
                Thumb = 0,
            };
            this.database.RatedReviews.Add(ratedReview);
            await this.database.SaveChangesAsync();
        }

        int oldRating = ratedReview.Thumb;
        ratedReview.Thumb = Math.Clamp(rating, -1, 1);
        if (oldRating == ratedReview.Thumb) return this.Ok();

        // if the user's rating changed then we recount the review's ratings to ensure accuracy
        List<int> reactions = await this.database.RatedReviews.Where(r => r.ReviewId == reviewerId).Select(r => r.Thumb).ToListAsync();
        int yay = 0;
        int boo = 0;
        foreach (int r in reactions)
        {
            switch (r)
            {
                case -1:
                    boo++;
                    break;
                case 1:
                    yay++;
                    break;
            }
        }

        review.ThumbsDown = boo;
        review.ThumbsUp = yay;

        await this.database.SaveChangesAsync();

        return this.Ok();
    }

    [HttpPost("deleteReview/user/{slotId:int}/{username}")]
    public async Task<IActionResult> DeleteReview(int slotId, string username)
    {
        GameTokenEntity token = this.GetToken();

        int creatorId = await this.database.Slots.Where(s => s.SlotId == slotId).Select(s => s.CreatorId).FirstOrDefaultAsync();
        if (creatorId == 0) return this.BadRequest();

        if (token.UserId != creatorId) return this.Unauthorized();

        int reviewerId = await this.database.UserIdFromUsername(username);
        if (reviewerId == 0) return this.BadRequest();

        ReviewEntity? review = await this.database.Reviews.FirstOrDefaultAsync(r => r.SlotId == slotId && r.ReviewerId == reviewerId);
        if (review == null) return this.BadRequest();

        review.Deleted = true;
        review.DeletedBy = DeletedBy.LevelAuthor;

        await this.database.SaveChangesAsync();
        return this.Ok();
    }
}