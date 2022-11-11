#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class UserPage : BaseLayout
{
    public List<Comment>? Comments;

    public bool CommentsEnabled;

    public bool IsProfileUserHearted;

    public List<Photo>? Photos;

    public User? ProfileUser;
    public UserPage(Database database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int userId)
    {
        this.ProfileUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == userId);
        if (this.ProfileUser == null) return this.NotFound();

        // Determine if user can view profile according to profileUser's privacy settings
        if (this.User == null || !this.User.IsAdmin)
        {
            switch (this.ProfileUser.ProfileVisibility)
            {
                case PrivacyType.PSN:
                {
                    if (this.User != null) return this.NotFound();

                    break;
                }
                case PrivacyType.Game:
                {
                    if (this.ProfileUser != this.User) return this.NotFound();

                    break;
                }
                case PrivacyType.All: break;
                default: throw new ArgumentOutOfRangeException();
            }
        }

        this.Photos = await this.Database.Photos.Include(p => p.Slot).OrderByDescending(p => p.Timestamp).Where(p => p.CreatorId == userId).Take(6).ToListAsync();

        this.CommentsEnabled = ServerConfiguration.Instance.UserGeneratedContentLimits.LevelCommentsEnabled && this.ProfileUser.CommentsEnabled;
        if (this.CommentsEnabled)
        {
            this.Comments = await this.Database.Comments.Include(p => p.Poster)
                .OrderByDescending(p => p.Timestamp)
                .Where(p => p.TargetId == userId && p.Type == CommentType.Profile)
                .Take(50)
                .ToListAsync();
        }
        else
        {
            this.Comments = new List<Comment>();
        }

        if (this.User == null) return this.Page();

        foreach (Comment c in this.Comments)
        {
            Reaction? reaction = await this.Database.Reactions.FirstOrDefaultAsync(r => r.UserId == this.User.UserId && r.TargetId == c.CommentId);
            if (reaction != null) c.YourThumb = reaction.Rating;
        }
        this.IsProfileUserHearted = await this.Database.HeartedProfiles.FirstOrDefaultAsync
                                        (u => u.UserId == this.User.UserId && u.HeartedUserId == this.ProfileUser.UserId) !=
                                    null;

        return this.Page();
    }
}