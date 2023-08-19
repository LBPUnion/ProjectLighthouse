using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Frames;

public class ReviewFrame : PaginatedFrame
{
    public List<ReviewEntity> Reviews = new();

    public bool CanViewReviews { get; set; }
    public bool ReviewsEnabled = ServerConfiguration.Instance.UserGeneratedContentLimits.LevelReviewsEnabled;

    public ReviewFrame(DatabaseContext database) : base(database)
    {
        this.ItemsPerPage = 10;
    }

    public async Task<IActionResult> OnGet([FromQuery] int page, int slotId)
    {
        this.CurrentPage = page;

        SlotEntity? slot = await this.Database.Slots.Include(s => s.Creator)
            .Where(s => s.SlotId == slotId)
            .FirstOrDefaultAsync();
        if (slot == null || slot.Creator == null) return this.BadRequest();

        this.CanViewReviews = slot.Creator.LevelVisibility.CanAccess(this.User != null,
            this.User == slot.Creator || this.User != null && this.User.IsModerator);

        if (!this.ReviewsEnabled || !this.CanViewReviews)
        {
            this.ClampPage();
            return this.Page();
        }

        List<int> blockedUsers = await this.Database.GetBlockedUsers(this.User?.UserId);

        IQueryable<ReviewEntity> reviewQuery = this.Database.Reviews.Where(r => r.SlotId == slotId)
            .Where(r => !blockedUsers.Contains(r.ReviewerId))
            .Include(r => r.Reviewer)
            .Where(r => r.Reviewer.PermissionLevel != PermissionLevel.Banned);

        this.TotalItems = await reviewQuery.CountAsync();

        this.ClampPage();

        this.Reviews = await reviewQuery.OrderByDescending(r => r.ThumbsUp - r.ThumbsDown)
            .ThenByDescending(r => r.Timestamp)
            .ApplyPagination(this.PageData)
            .ToListAsync();

        return this.Page();
    }

}