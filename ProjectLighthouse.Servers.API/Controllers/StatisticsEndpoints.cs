using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.API.Responses;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.API.Controllers;

/// <summary>
/// A collection of endpoints relating to statistics.
/// </summary>
public class StatisticsEndpoints : ApiEndpointController
{

    private readonly DatabaseContext database;

    public StatisticsEndpoints(DatabaseContext database)
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

    private static readonly List<GameVersion> gameVersions = new()
    {
        GameVersion.LittleBigPlanet1,
        GameVersion.LittleBigPlanet2,
        GameVersion.LittleBigPlanet3,
        GameVersion.LittleBigPlanetVita,
        GameVersion.LittleBigPlanetPSP,
    };

    /// <summary>
    /// Get player counts for each individual title
    /// </summary>
    /// <returns>An instance of PlayerCountResponse</returns>
    [HttpGet("playerCount")]
    [ProducesResponseType(typeof(PlayerCountResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlayerCounts()
    {
        List<PlayerCountObject> gameList = new();
        foreach (GameVersion version in gameVersions)
        {
            gameList.Add(new PlayerCountObject
            {
                Game = version.ToString(),
                PlayerCount = await StatisticsHelper.RecentMatchesForGame(this.database, version),
            });
        }
        PlayerCountResponse response = new()
        {
            TotalPlayerCount = await StatisticsHelper.RecentMatches(this.database),
            Games = gameList,
        };

        return this.Ok(response);
    }
}