using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/plain")]
public class StatisticsController : ControllerBase
{

    private readonly DatabaseContext database;

    public StatisticsController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpGet("playersInPodCount")]
public IActionResult PlayersInPodCount() => this.Ok(StatisticsHelper.RoomCountForPlatform(this.GetToken().Platform).ToString());

    [HttpGet("totalPlayerCount")]
    public async Task<IActionResult> TotalPlayerCount() => this.Ok((await StatisticsHelper.RecentMatchesForGame(this.database, this.GetToken().GameVersion)).ToString());

    [HttpGet("planetStats")]
    [Produces("text/xml")]
    public async Task<IActionResult> PlanetStats()
    {
        int totalSlotCount = await StatisticsHelper.SlotCountForGame(this.database, this.GetToken().GameVersion);
        int mmPicksCount = await StatisticsHelper.TeamPickCountForGame(this.database, this.GetToken().GameVersion);

        return this.Ok(new PlanetStatsResponse(totalSlotCount, mmPicksCount));
    }

    [HttpGet("planetStats/totalLevelCount")]
    public async Task<IActionResult> TotalLevelCount() => this.Ok((await StatisticsHelper.SlotCountForGame(this.database, this.GetToken().GameVersion)).ToString());
}
