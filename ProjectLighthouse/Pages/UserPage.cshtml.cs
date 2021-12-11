#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages
{
    public class UserPage : BaseLayout
    {
        public List<Comment>? Comments;

        public bool IsProfileUserHearted;

        public List<Photo>? Photos;

        public User? ProfileUser;
        public UserPage(Database database) : base(database)
        {}

        public async Task<IActionResult> OnGet([FromRoute] int userId)
        {
            this.ProfileUser = await this.Database.Users.FirstOrDefaultAsync(u => u.UserId == userId);
            if (this.ProfileUser == null) return this.NotFound();

            this.Photos = await this.Database.Photos.OrderByDescending(p => p.Timestamp).Where(p => p.CreatorId == userId).Take(5).ToListAsync();
            this.Comments = await this.Database.Comments.Include
                    (p => p.Poster)
                .Include(p => p.Target)
                .OrderByDescending(p => p.Timestamp)
                .Where(p => p.TargetUserId == userId)
                .Take(50)
                .ToListAsync();

            if (this.User != null)
                this.IsProfileUserHearted = await this.Database.HeartedProfiles.FirstOrDefaultAsync
                                                (u => u.UserId == this.User.UserId && u.HeartedUserId == this.ProfileUser.UserId) !=
                                            null;

            return this.Page();
        }
    }
}