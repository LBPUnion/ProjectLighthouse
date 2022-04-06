#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Helpers.Extensions;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Reviews;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers.GameApi.Slots;

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

    // LBP1 rating
    [HttpPost("rate/user/{slotId}")]
    public async Task<IActionResult> Rate(int slotId, [FromQuery] int rating)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.Include(s => s.Creator).Include(s => s.Location).FirstOrDefaultAsync(s => s.SlotId == slotId);
        if (slot == null) return this.StatusCode(403, "");

        RatedLevel? ratedLevel = await this.database.RatedLevels.FirstOrDefaultAsync(r => r.SlotId == slotId && r.UserId == user.UserId);
        if (ratedLevel == null)
        {
            ratedLevel = new RatedLevel
            {
                SlotId = slotId,
                UserId = user.UserId,
                Rating = 0,
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
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.Include(s => s.Creator).Include(s => s.Location).FirstOrDefaultAsync(s => s.SlotId == slotId);
        if (slot == null) return this.StatusCode(403, "");

        RatedLevel? ratedLevel = await this.database.RatedLevels.FirstOrDefaultAsync(r => r.SlotId == slotId && r.UserId == user.UserId);
        if (ratedLevel == null)
        {
            ratedLevel = new RatedLevel
            {
                SlotId = slotId,
                UserId = user.UserId,
                RatingLBP1 = 0,
            };
            this.database.RatedLevels.Add(ratedLevel);
        }

        ratedLevel.Rating = Math.Clamp(rating, -1, 1);

        Review? review = await this.database.Reviews.FirstOrDefaultAsync(r => r.SlotId == slotId && r.ReviewerId == user.UserId);
        if (review != null) review.Thumb = ratedLevel.Rating;

        await this.database.SaveChangesAsync();

        return this.Ok();
    }

    [HttpPost("postReview/user/{slotId:int}")]
    public async Task<IActionResult> PostReview(int slotId)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        Review? newReview = await this.getReviewFromBody();
        if (newReview == null) return this.BadRequest();

        if (newReview.Text.Length > 100) return this.BadRequest();

        Review? review = await this.database.Reviews.FirstOrDefaultAsync(r => r.SlotId == slotId && r.ReviewerId == user.UserId);

        if (review == null)
        {
            review = new Review
            {
                SlotId = slotId,
                ReviewerId = user.UserId,
                DeletedBy = DeletedBy.None,
                ThumbsUp = 0,
                ThumbsDown = 0,
            };
            this.database.Reviews.Add(review);
        }
        review.Thumb = newReview.Thumb;
        review.LabelCollection = newReview.LabelCollection;
        review.Text = newReview.Text;
        review.Deleted = false;
        review.Timestamp = TimeHelper.UnixTimeMilliseconds();

        // sometimes the game posts/updates a review rating without also calling dpadrate/user/etc (why??)
        RatedLevel? ratedLevel = await this.database.RatedLevels.FirstOrDefaultAsync(r => r.SlotId == slotId && r.UserId == user.UserId);
        if (ratedLevel == null)
        {
            ratedLevel = new RatedLevel
            {
                SlotId = slotId,
                UserId = user.UserId,
                RatingLBP1 = 0,
            };
            this.database.RatedLevels.Add(ratedLevel);
        }

        ratedLevel.Rating = newReview.Thumb;

        await this.database.SaveChangesAsync();

        return this.Ok();
    }

    [HttpGet("reviewsFor/user/{slotId:int}")]
    public async Task<IActionResult> ReviewsFor(int slotId, [FromQuery] int pageStart = 1, [FromQuery] int pageSize = 10)
    {
        (User, GameToken)? userAndToken = await this.database.UserAndGameTokenFromRequest(this.Request);

        if (userAndToken == null) return this.StatusCode(403, "");

        // ReSharper disable once PossibleInvalidOperationException
        User user = userAndToken.Value.Item1;
        GameToken gameToken = userAndToken.Value.Item2;

        GameVersion gameVersion = gameToken.GameVersion;

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slotId);
        if (slot == null) return this.BadRequest();

        IQueryable<Review?> reviews = this.database.Reviews.ByGameVersion(gameVersion, true)
            .Where(r => r.SlotId == slotId)
            .Include(r => r.Reviewer)
            .Include(r => r.Slot)
            .OrderByDescending(r => r.ThumbsUp - r.ThumbsDown)
            .ThenByDescending(r => r.Timestamp)
            .Skip(pageStart - 1)
            .Take(pageSize);

        List<Review?> reviewList = reviews.ToList();

        string inner = reviewList
            .Aggregate
            (
                string.Empty,
                (current, review) =>
                {
                    if (review == null) return current;

                    RatedReview? yourThumb = this.database.RatedReviews.FirstOrDefault(r => r.ReviewId == review.ReviewId && r.UserId == user.UserId);
                    return current + review.Serialize(null, yourThumb);
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
                    "hint", reviewList.LastOrDefault()!.Timestamp // not sure
                },
            }
        );
        return this.Ok(response);
    }

    [HttpGet("reviewsBy/{username}")]
    public async Task<IActionResult> ReviewsBy(string username, [FromQuery] int pageStart = 1, [FromQuery] int pageSize = 10)
    {
        (User, GameToken)? userAndToken = await this.database.UserAndGameTokenFromRequest(this.Request);

        if (userAndToken == null) return this.StatusCode(403, "");

        // ReSharper disable once PossibleInvalidOperationException
        User user = userAndToken.Value.Item1;
        GameToken gameToken = userAndToken.Value.Item2;

        GameVersion gameVersion = gameToken.GameVersion;

        IEnumerable<Review?> reviews = this.database.Reviews.ByGameVersion(gameVersion, true)
            .Include(r => r.Reviewer)
            .Include(r => r.Slot)
            .Where(r => r.Reviewer!.Username == username)
            .OrderByDescending(r => r.Timestamp)
            .Skip(pageStart - 1)
            .Take(pageSize);

        List<Review?> reviewList = reviews.ToList();

        string inner = reviewList.Aggregate
        (
            string.Empty,
            (current, review) =>
            {
                if (review == null) return current;

                RatedReview? ratedReview = this.database.RatedReviews.FirstOrDefault(r => r.ReviewId == review.ReviewId && r.UserId == user.UserId);
                return current + review.Serialize(null, ratedReview);
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
                    "hint", reviewList.LastOrDefault()!.Timestamp // Seems to be the timestamp of oldest
                },
            }
        );

        return this.Ok(response);
    }

    [HttpPost("rateReview/user/{slotId:int}/{username}")]
    public async Task<IActionResult> RateReview(int slotId, string username, [FromQuery] int rating = 0)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        User? reviewer = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (reviewer == null) return this.StatusCode(400, "");

        Review? review = await this.database.Reviews.FirstOrDefaultAsync(r => r.SlotId == slotId && r.ReviewerId == reviewer.UserId);
        if (review == null) return this.StatusCode(400, "");

        RatedReview? ratedReview = await this.database.RatedReviews.FirstOrDefaultAsync(r => r.ReviewId == review.ReviewId && r.UserId == user.UserId);
        if (ratedReview == null)
        {
            ratedReview = new RatedReview
            {
                ReviewId = review.ReviewId,
                UserId = user.UserId,
                Thumb = 0,
            };
            this.database.RatedReviews.Add(ratedReview);
            await this.database.SaveChangesAsync();
        }

        int oldRating = ratedReview.Thumb;
        ratedReview.Thumb = Math.Clamp(rating, -1, 1);
        if (oldRating == ratedReview.Thumb) return this.Ok();

        // if the user's rating changed then we recount the review's ratings to ensure accuracy
        List<RatedReview> reactions = await this.database.RatedReviews.Where(r => r.ReviewId == review.ReviewId).ToListAsync();
        int yay = 0;
        int boo = 0;
        foreach (RatedReview r in reactions)
        {
            switch (r.Thumb)
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
        User? reviewer = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
        if (reviewer == null) return this.StatusCode(403, "");

        Review? review = await this.database.Reviews.FirstOrDefaultAsync(r => r.SlotId == slotId && r.ReviewerId == reviewer.UserId);
        if (review == null) return this.StatusCode(403, "");

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