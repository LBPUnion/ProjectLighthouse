#nullable enable
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages
{
    public class LandingPage : BaseLayout
    {
        public LandingPage(Database database) : base(database)
        {}

        public int PlayersOnline;

        [UsedImplicitly]
        public async Task<IActionResult> OnGet()
        {
            this.PlayersOnline = await StatisticsHelper.RecentMatches();
            return this.Page();
        }
    }
}