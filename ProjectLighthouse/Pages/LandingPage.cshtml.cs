#nullable enable
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages
{
    public class LandingPage : BaseLayout
    {
        public LandingPage(Database database) : base(database)
        {}

        public int PlayersOnlineCount;
        public List<User> PlayersOnline;

        [UsedImplicitly]
        public async Task<IActionResult> OnGet()
        {
            this.PlayersOnlineCount = await StatisticsHelper.RecentMatches();

            List<int> userIds = await this.Database.LastMatches.Where(l => TimestampHelper.Timestamp - l.Timestamp < 300).Select(l => l.UserId).ToListAsync();

            this.PlayersOnline = await this.Database.Users.Where(u => userIds.Contains(u.UserId)).ToListAsync();
            return this.Page();
        }
    }
}