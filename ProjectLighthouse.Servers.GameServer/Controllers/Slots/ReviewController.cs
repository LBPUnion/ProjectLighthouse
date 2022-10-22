#nullable enable
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Administration;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Reviews;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class ReviewController : ControllerBase
{
    private readonly Database database;

    public ReviewController(Database database)
    {
        this.database = database;
    }

    [HttpGet("review/user/{slotId:int}/{userName}")]
    public async Task<IActionResult> GetReview(int slotId, string userName)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403);
        if (userName == null) return this.BadRequest();
        Review? targetReview = await this.database.Reviews.Include(r => r.Reviewer).FirstOrDefaultAsync(r => r.SlotId == slotId && r.Reviewer!.Username == userName);
        return this.Ok(targetReview?.Serialize());
    }

    // LBP1 rating
    [HttpPost("rate/user/{slotId:int}")]
    public async Task<IActionResult> Rate(int slotId, [FromQuery] int rating)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.Include(s => s.Creator).Include(s => s.Location).FirstOrDefaultAsync(s => s.SlotId == slotId);
        if (slot == null) return this.StatusCode(403, "");

        RatedLevel? ratedLevel = await this.database.RatedLevels.FirstOrDefaultAsync(r => r.SlotId == slotId && r.UserId == token.UserId);
        if (ratedLevel == null)
        {
            ratedLevel = new RatedLevel
            {
                SlotId = slotId,
                UserId = token.UserId,
                Rating = 0,
                TagLBP1 = "",
            };
            this.database.RatedLevels.Add(ratedLevel);
        }

        ratedLevel.RatingLBP1 = Math.Max(Math.Min(5, rating), 0);

        await this.database.SaveChangesAsync();

        return this.Ok();
    }

    // LBP2 and beyond rating
    [HttpPost("dpadrate/user/{slotId:int}")]
    public async Task<IActionResult> DPadRate(int slotId, [FromQuery] int rating)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.Include(s => s.Location).FirstOrDefaultAsync(s => s.SlotId == slotId);
        if (slot == null) return this.StatusCode(403, "");

        RatedLevel? ratedLevel = await this.database.RatedLevels.FirstOrDefaultAsync(r => r.SlotId == slotId && r.UserId == token.UserId);
        if (ratedLevel == null)
        {
            ratedLevel = new RatedLevel
            {
                SlotId = slotId,
                UserId = token.UserId,
                RatingLBP1 = 0,
                TagLBP1 = "",
            };
            this.database.RatedLevels.Add(ratedLevel);
        }

        ratedLevel.Rating = Math.Clamp(rating, -1, 1);

        Review? review = await this.database.Reviews.FirstOrDefaultAsync(r => r.SlotId == slotId && r.ReviewerId == token.UserId);
        if (review != null) review.Thumb = ratedLevel.Rating;

        await this.database.SaveChangesAsync();

        return this.Ok();
    }

    [HttpPost("postReview/user/{slotId:int}")]
    public async Task<IActionResult> PostReview(int slotId)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        Review? newReview = await this.getReviewFromBody();
        if (newReview == null) return this.BadRequest();

        if (newReview.Text.Length > 512) return this.BadRequest();

        Review? review = await this.database.Reviews.FirstOrDefaultAsync(r => r.SlotId == slotId && r.ReviewerId == token.UserId);

        if (review == null)
        {
            review = new Review
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
        review.Timestamp = TimeHelper.UnixTimeMilliseconds();

        // sometimes the game posts/updates a review rating without also calling dpadrate/user/etc (why??)
        RatedLevel? ratedLevel = await this.database.RatedLevels.FirstOrDefaultAsync(r => r.SlotId == slotId && r.UserId == token.UserId);
        if (ratedLevel == null)
        {
            ratedLevel = new RatedLevel
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
    public async Task<IActionResult> ReviewsFor(int slotId, [FromQuery] int pageStart = 1, [FromQuery] int pageSize = 10)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        if (pageSize <= 0) return this.BadRequest();

        GameVersion gameVersion = token.GameVersion;

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slotId);
        if (slot == null) return this.BadRequest();

        IQueryable<Review?> reviews = this.database.Reviews.ByGameVersion(gameVersion, true)
            .Where(r => r.SlotId == slotId)
            .Include(r => r.Reviewer)
            .Include(r => r.Slot)
            .OrderByDescending(r => r.ThumbsUp - r.ThumbsDown)
            .ThenByDescending(r => r.Timestamp)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(pageSize);

        List<Review?> reviewList = reviews.ToList();

        string inner = reviewList.Aggregate
        (
            string.Empty,
            (current, review) =>
            {
                if (review == null) return current;

                RatedReview? yourThumb = this.database.RatedReviews.FirstOrDefault(r => r.ReviewId == review.ReviewId && r.UserId == token.UserId);
                return current + review.Serialize(yourThumb);
            }
        );
        string response = LbpSerializer.TaggedStringElement
        (
            "reviews",
            inner,
            new Dictionary<string, object>
            {
                {
                    "hint_start", pageStart + pageSize
                },
                {
                    "hint", reviewList.LastOrDefault()?.Timestamp ?? 0
                },
            }
        );
        return this.Ok(response);
    }

    [HttpGet("reviewsBy/{username}")]
    public async Task<IActionResult> ReviewsBy(string username, [FromQuery] int pageStart = 1, [FromQuery] int pageSize = 10)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        if (pageSize <= 0) return this.BadRequest();

        GameVersion gameVersion = token.GameVersion;

        int targetUserId = await this.database.Users.Where(u => u.Username == username).Select(u => u.UserId).FirstOrDefaultAsync();

        if (targetUserId == 0) return this.BadRequest();

        IEnumerable<Review?> reviews = this.database.Reviews.ByGameVersion(gameVersion, true)
            .Include(r => r.Reviewer)
            .Include(r => r.Slot)
            .Where(r => r.ReviewerId == targetUserId)
            .OrderByDescending(r => r.Timestamp)
            .Skip(Math.Max(0, pageStart - 1))
            .Take(pageSize);

        List<Review?> reviewList = reviews.ToList();

        string inner = reviewList.Aggregate
        (
            string.Empty,
            (current, review) =>
            {
                if (review == null) return current;

                RatedReview? ratedReview = this.database.RatedReviews.FirstOrDefault(r => r.ReviewId == review.ReviewId && r.UserId == token.UserId);
                return current + review.Serialize(ratedReview);
            }
        );

        string response = LbpSerializer.TaggedStringElement
        (
            "reviews",
            inner,
            new Dictionary<string, object>
            {
                {
                    "hint_start", pageStart
                },
                {
                    "hint", reviewList.LastOrDefault()?.Timestamp ?? 0 // Seems to be the timestamp of oldest
                },
            }
        );

        return this.Ok(response);
    }

    [HttpPost("rateReview/user/{slotId:int}/{username}")]
    public async Task<IActionResult> RateReview(int slotId, string username, [FromQuery] int rating = 0)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        int reviewerId = await this.database.Users.Where(u => u.Username == username).Select(u => u.UserId).FirstOrDefaultAsync();
        if (reviewerId == 0) return this.StatusCode(400, "");

        Review? review = await this.database.Reviews.FirstOrDefaultAsync(r => r.SlotId == slotId && r.ReviewerId == reviewerId);
        if (review == null) return this.StatusCode(400, "");

        RatedReview? ratedReview = await this.database.RatedReviews.FirstOrDefaultAsync(r => r.ReviewId == review.ReviewId && r.UserId == token.UserId);
        if (ratedReview == null)
        {
            ratedReview = new RatedReview
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
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        int creatorId = await this.database.Slots.Where(s => s.SlotId == slotId).Select(s => s.CreatorId).FirstOrDefaultAsync();
        if (creatorId == 0) return this.StatusCode(400, "");

        if (token.UserId != creatorId) return this.StatusCode(403, "");

        int reviewerId = await this.database.Users.Where(u => u.Username == username).Select(u => u.UserId).FirstOrDefaultAsync();
        if (reviewerId == 0) return this.StatusCode(400, "");

        Review? review = await this.database.Reviews.FirstOrDefaultAsync(r => r.SlotId == slotId && r.ReviewerId == reviewerId);
        if (review == null) return this.StatusCode(400, "");

        review.Deleted = true;
        review.DeletedBy = DeletedBy.LevelAuthor;

        await this.database.SaveChangesAsync();
        return this.Ok();
    }

    private async Task<Review?> getReviewFromBody()
    {
        this.Request.Body.Position = 0;
        string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

        XmlSerializer serializer = new(typeof(Review));
        Review? review = (Review?)serializer.Deserialize(new StringReader(bodyString));
        SanitizationHelper.SanitizeStringsInClass(review);
        return review;
    }
}