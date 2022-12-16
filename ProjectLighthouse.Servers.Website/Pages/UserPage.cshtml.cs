#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class UserPage : BaseLayout
{
    public List<Comment>? Comments;

    public bool CommentsEnabled;

    public bool IsProfileUserHearted;

    public List<Photo>? Photos;
    public List<Slot>? Slots;

    public List<Slot>? HeartedSlots;
    public List<Slot>? QueuedSlots;

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

        this.Photos = await this.Database.Photos.Include(p => p.Slot)
            .OrderByDescending(p => p.Timestamp)
            .Where(p => p.CreatorId == userId)
            .Take(6)
            .ToListAsync();

        this.Slots = await this.Database.Slots.Include(p => p.Creator)
            .OrderByDescending(s => s.LastUpdated)
            .Where(p => p.CreatorId == userId)
            .Take(10)
            .ToListAsync();

        if (this.User == this.ProfileUser)
        {
            this.QueuedSlots = await this.Database.QueuedLevels.Include(h => h.Slot)
                .Where(h => this.User != null && h.UserId == this.User.UserId)
                .Select(h => h.Slot)
                .Where(s => s.Type == SlotType.User)
                .Take(10)
                .ToListAsync();
            this.HeartedSlots = await this.Database.HeartedLevels.Include(h => h.Slot)
                .Where(h => this.User != null && h.UserId == this.User.UserId)
                .Select(h => h.Slot)
                .Where(s => s.Type == SlotType.User)
                .Take(10)
                .ToListAsync();
            Console.WriteLine("user is profileuser");
        }

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
            Reaction? reaction = await this.Database.Reactions.Where(r => r.TargetId == c.TargetId)
                .Where(r => r.UserId == this.User.UserId)
                .FirstOrDefaultAsync();
            if (reaction != null) c.YourThumb = reaction.Rating;
        }

        this.IsProfileUserHearted = await this.Database.HeartedProfiles
            .Where(h => h.HeartedUserId == this.ProfileUser.UserId)
            .Where(h => h.UserId == this.User.UserId)
            .AnyAsync();

        return this.Page();
    }
}