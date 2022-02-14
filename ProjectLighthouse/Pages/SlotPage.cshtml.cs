#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages;

public class SlotPage : BaseLayout
{
    public List<Comment> Comments;

    public bool CommentsEnabled = ServerSettings.Instance.LevelCommentsEnabled;

    public Slot Slot;
    public SlotPage([NotNull] Database database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int id)
    {
        Slot? slot = await this.Database.Slots.Include(s => s.Creator).FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        this.Slot = slot;

        if (this.CommentsEnabled)
        {
            this.Comments = await this.Database.Comments.Include(p => p.Poster)
                .OrderByDescending(p => p.Timestamp)
                .Where(c => c.TargetId == id && c.Type == CommentType.Level)
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

        return this.Page();
    }
}