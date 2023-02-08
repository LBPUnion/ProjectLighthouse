using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.Servers.API.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.API.Controllers;

/// <summary>
/// A collection of endpoints relating to statistics.
/// </summary>
public class StatisticsEndpoints : ApiEndpointController
{

    private readonly Database database;

    public StatisticsEndpoints(Database database)
    {
        this.database = database;
    }

    /// <summary>
    /// Gets everything that StatisticsHelper provides.
    /// </summary>
    /// <returns>An instance of StatisticsResponse</returns>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(StatisticsResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetStatistics()
        => this.Ok
        (
            new StatisticsResponse
            {
                Photos = await StatisticsHelper.PhotoCount(this.database),
                Slots = await StatisticsHelper.SlotCount(this.database),
                Users = await StatisticsHelper.UserCount(this.database),
                RecentMatches = await StatisticsHelper.RecentMatches(this.database),
                TeamPicks = await StatisticsHelper.TeamPickCount(this.database),
            }
        );

    /// <summary>
    /// Get player counts for each individual title
    /// </summary>
    /// <returns>An instance of PlayerCountResponse</returns>
    [HttpGet("playerCount")]
    [ProducesResponseType(typeof(PlayerCountResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlayerCounts() =>
        this.Ok(new PlayerCountResponse
        {
            PlayerCounts = 
            {
                { 
                    GameVersion.LittleBigPlanet1,
                    await StatisticsHelper.RecentMatchesForGame(this.database, GameVersion.LittleBigPlanet1)
                },
                {
                    GameVersion.LittleBigPlanet2,
                    await StatisticsHelper.RecentMatchesForGame(this.database, GameVersion.LittleBigPlanet2)
                },
                {
                    GameVersion.LittleBigPlanet3,
                    await StatisticsHelper.RecentMatchesForGame(this.database, GameVersion.LittleBigPlanet3)
                },
                {
                    GameVersion.LittleBigPlanetVita,
                    await StatisticsHelper.RecentMatchesForGame(this.database, GameVersion.LittleBigPlanetVita)
                },
                {
                    GameVersion.LittleBigPlanetPSP,
                    await StatisticsHelper.RecentMatchesForGame(this.database, GameVersion.LittleBigPlanetPSP)
                },
            },
        });
}