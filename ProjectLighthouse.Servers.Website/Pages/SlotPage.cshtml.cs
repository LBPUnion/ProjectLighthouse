#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class SlotPage : BaseLayout
{
    public Dictionary<CommentEntity, RatedCommentEntity?> Comments = new();
    public List<ReviewEntity> Reviews = new();
    public List<PhotoEntity> Photos = new();
    public List<ScoreEntity> Scores = new();

    public bool CommentsEnabled;
    public readonly bool ReviewsEnabled = ServerConfiguration.Instance.UserGeneratedContentLimits.LevelReviewsEnabled;

    public SlotEntity? Slot;
    public SlotPage(DatabaseContext database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int id)
    {
        SlotEntity? slot = await this.Database.Slots.Include(s => s.Creator)
            .Where(s => s.Type == SlotType.User)
            .FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();
        System.Diagnostics.Debug.Assert(slot.Creator != null);

        // Determine if user can view slot according to creator's privacy settings
        if (this.User == null || !this.User.IsAdmin)
        {
            switch (slot.Creator.ProfileVisibility)
            {
                case PrivacyType.PSN:
                {
                    if (this.User != null) return this.NotFound();

                    break;
                }
                case PrivacyType.Game:
                {
                    if (this.User == null || slot.Creator != this.User) return this.NotFound();

                    break;
                }
                case PrivacyType.All: break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        if ((slot.Hidden || slot.SubLevel && (this.User == null && this.User != slot.Creator)) && !(this.User?.IsModerator ?? false))
            return this.NotFound();

        this.Slot = slot;

        List<int> blockedUsers = this.User == null
            ? new List<int>()
            : await (
                from blockedProfile in this.Database.BlockedProfiles
                where blockedProfile.UserId == this.User.UserId
                select blockedProfile.BlockedUserId).ToListAsync();
        
        this.CommentsEnabled = ServerConfiguration.Instance.UserGeneratedContentLimits.LevelCommentsEnabled && this.Slot.CommentsEnabled;
        if (this.CommentsEnabled)
        {
            this.Comments = await this.Database.Comments.Include(p => p.Poster)
                .OrderByDescending(p => p.Timestamp)
                .Where(c => c.TargetId == id && c.Type == CommentType.Level)
                .Where(c => !blockedUsers.Contains(c.PosterUserId))
                .Include(c => c.Poster)
                .Where(c => c.Poster.PermissionLevel != PermissionLevel.Banned)
                .Take(50)
                .ToDictionaryAsync(c => c,  _ => (RatedCommentEntity?)null);
        }
        else
        {
            this.Comments = new Dictionary<CommentEntity, RatedCommentEntity?>();
        }

        if (this.ReviewsEnabled)
        {
            this.Reviews = await this.Database.Reviews.Include(r => r.Reviewer)
                .OrderByDescending(r => r.ThumbsUp - r.ThumbsDown)
                .ThenByDescending(r => r.Timestamp)
                .Where(r => r.SlotId == id)
                .Where(r => !blockedUsers.Contains(r.ReviewerId))
                .Take(50)
                .ToListAsync();
        }
        else
        {
            this.Reviews = new List<ReviewEntity>();
        }

        this.Photos = await this.Database.Photos.Include(p => p.Creator)
            .Include(p => p.PhotoSubjects)
            .ThenInclude(ps => ps.User)
            .OrderByDescending(p => p.Timestamp)
            .Where(r => r.SlotId == id)
            .Take(10)
            .ToListAsync();

        this.Scores = await this.Database.Scores.OrderByDescending(s => s.Points)
            .ThenBy(s => s.ScoreId)
            .Where(s => s.SlotId == id)
            .Take(10)
            .ToListAsync();

        if (this.User == null) return this.Page();

        foreach (KeyValuePair<CommentEntity, RatedCommentEntity?> kvp in this.Comments)
        {
            RatedCommentEntity? reaction = await this.Database.RatedComments.FirstOrDefaultAsync(r => r.UserId == this.User.UserId && r.CommentId == kvp.Key.CommentId);
            this.Comments[kvp.Key] = reaction;
        }

        return this.Page();
    }
}
