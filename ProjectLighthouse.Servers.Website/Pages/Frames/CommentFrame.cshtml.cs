using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Frames;

public class CommentFrame : BaseFrame
{
    public CommentFrame(DatabaseContext database) : base(database)
    { }

    public Dictionary<CommentEntity, RatedCommentEntity?> Comments = new();

    public bool CommentsEnabled { get; set; }

    public int PageOwner { get; set; }

    public async Task<IActionResult> OnGet(string type, int id)
    {
        CommentType? commentType = type switch
        {
            "slot" => CommentType.Level,
            "user" => CommentType.Profile,
            _ => null,
        };
        switch (commentType)
        {
            case CommentType.Level:
            {
                this.CommentsEnabled = ServerConfiguration.Instance.UserGeneratedContentLimits.LevelCommentsEnabled;
                SlotEntity? slot = await this.Database.Slots.FindAsync(id);
                if (slot == null) return this.BadRequest();
                this.PageOwner = slot.CreatorId;
                this.CommentsEnabled &= slot.CommentsEnabled;
                break;
            }
            case CommentType.Profile:
            {
                this.CommentsEnabled = ServerConfiguration.Instance.UserGeneratedContentLimits.LevelCommentsEnabled;
                UserEntity? user = await this.Database.Users.FindAsync(id);
                if (user == null) return this.BadRequest();
                this.PageOwner = user.UserId;
                this.CommentsEnabled &= user.CommentsEnabled;
                break;
            }
            default: return this.BadRequest();
        }

        if (this.CommentsEnabled)
        {
            List<int> blockedUsers = this.User == null
                ? new List<int>()
                : await (
                    from blockedProfile in this.Database.BlockedProfiles
                    where blockedProfile.UserId == this.User.UserId
                    select blockedProfile.BlockedUserId).ToListAsync();

            this.Comments = await this.Database.Comments.Include(p => p.Poster)
                .OrderByDescending(p => p.Timestamp)
                .Where(c => c.TargetId == id && c.Type == commentType)
                .Where(c => !blockedUsers.Contains(c.PosterUserId))
                .Include(c => c.Poster)
                .Where(c => c.Poster.PermissionLevel != PermissionLevel.Banned)
                .Take(50)
                .ToDictionaryAsync(c => c, _ => (RatedCommentEntity?)null);

            if (this.User == null) return this.Page();

            foreach (KeyValuePair<CommentEntity, RatedCommentEntity?> kvp in this.Comments)
            {
                RatedCommentEntity? reaction = await this.Database.RatedComments.FirstOrDefaultAsync(r =>
                    r.UserId == this.User.UserId && r.CommentId == kvp.Key.CommentId);
                this.Comments[kvp.Key] = reaction;
            }
        }
        else
        {
            this.Comments = new Dictionary<CommentEntity, RatedCommentEntity?>();
        }

        return this.Page();
    }
}