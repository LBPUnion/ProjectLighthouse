using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Interaction;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Frames;

public class CommentFrame : PaginatedFrame
{
    public CommentFrame(DatabaseContext database) : base(database)
    {
        this.ItemsPerPage = 10;
    }

    public Dictionary<CommentEntity, RatedCommentEntity?> Comments = new();

    public int Id { get; set; }
    public string Type { get; set; } = "";

    public bool CommentsEnabled { get; set; }
    public bool CanViewComments { get; set; }

    public int PageOwner { get; set; }

    public async Task<IActionResult> OnGet([FromQuery] int page, string type, int id)
    {
        this.Type = type;
        this.Id = id;
        this.CurrentPage = page;
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
                SlotEntity? slot = await this.Database.Slots.Include(s => s.Creator)
                    .FirstOrDefaultAsync(s => s.SlotId == id);
                if (slot == null || slot.Creator == null) return this.BadRequest();
                this.PageOwner = slot.CreatorId;
                this.CanViewComments = slot.Creator.LevelVisibility.CanAccess(this.User != null,
                    slot.Creator == this.User || this.User != null && this.User.IsModerator);
                this.CommentsEnabled = ServerConfiguration.Instance.UserGeneratedContentLimits.LevelCommentsEnabled &&
                                       slot.CommentsEnabled;
                break;
            }
            case CommentType.Profile:
            {
                UserEntity? user = await this.Database.Users.FindAsync(id);
                if (user == null) return this.BadRequest();
                this.PageOwner = user.UserId;
                this.CanViewComments = user.ProfileVisibility.CanAccess(this.User != null,
                    user == this.User || this.User != null && this.User.IsModerator);
                this.CommentsEnabled = ServerConfiguration.Instance.UserGeneratedContentLimits.ProfileCommentsEnabled &&
                                       user.CommentsEnabled;
                break;
            }
            default: return this.BadRequest();
        }

        if (this.CommentsEnabled && this.CanViewComments)
        {
            List<int> blockedUsers = await this.Database.GetBlockedUsers(this.User?.UserId);

            IQueryable<CommentEntity> commentQuery = this.Database.Comments.Include(p => p.Poster)
                .Where(c => c.Poster.PermissionLevel != PermissionLevel.Banned)
                .Where(c => c.TargetId == id && c.Type == commentType)
                .Where(c => !blockedUsers.Contains(c.PosterUserId));

            this.TotalItems = await commentQuery.CountAsync();

            this.ClampPage();

            int userId = this.User?.UserId ?? 0;

            this.Comments = await commentQuery.OrderByDescending(c => c.Timestamp)
                .ApplyPagination(this.PageData)
                .Select(c => new
                {
                    Comment = c,
                    YourRating = this.Database.RatedComments.FirstOrDefault(r => r.CommentId == c.CommentId && r.UserId == userId),
                })
                .ToDictionaryAsync(c => c.Comment, c => c.YourRating);
        }
        else
        {
            this.ClampPage();
        }

        return this.Page();
    }
}