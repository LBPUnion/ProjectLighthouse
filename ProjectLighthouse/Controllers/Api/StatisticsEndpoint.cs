using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Api;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Controllers.Api;

public class StatisticsEndpoint : ApiEndpoint
{
    [HttpGet("statistics")]
    public async Task<IActionResult> OnGet()
        => this.Ok
        (
            new StatisticsResponse
            {
                Photos = await StatisticsHelper.PhotoCount(),
                Slots = await StatisticsHelper.SlotCount(),
                Users = await StatisticsHelper.UserCount(),
                RecentMatches = await StatisticsHelper.RecentMatches(),
                TeamPicks = await StatisticsHelper.TeamPickCount(),
            }
        );
}