using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Filter.Filters;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.API.Responses;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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
                Slots = await StatisticsHelper.SlotCount(this.database, new SlotQueryBuilder()),
                Users = await StatisticsHelper.UserCount(this.database),
                RecentMatches = await StatisticsHelper.RecentMatches(this.database),
                TeamPicks = await StatisticsHelper.SlotCount(this.database, new SlotQueryBuilder().AddFilter(new TeamPickFilter())),
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

    private static readonly List<Platform> platforms = new()
    {
        Platform.PS3,
        Platform.RPCS3,
        Platform.Vita,
        Platform.PSP,
    };

    /// <summary>
    /// Get player counts for each individual title
    /// </summary>
    /// <returns>An instance of PlayerCountByGameResponse</returns>
    [HttpGet("playerCount")]
    [HttpGet("playerCount/game")]
    [ProducesResponseType(typeof(PlayerCountByGameResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlayerCounts()
    {
        List<PlayerCountObject> gameList = new();
        foreach (GameVersion version in gameVersions)
        {
            gameList.Add(new PlayerCountByGameObject
            {
                Game = version.ToString(),
                PlayerCount = await StatisticsHelper.RecentMatches(this.database, l => l.GameVersion == version),
            });
        }
        PlayerCountByGameResponse response = new()
        {
            TotalPlayerCount = await StatisticsHelper.RecentMatches(this.database),
            Games = gameList,
        };

        return this.Ok(response);
    }

    /// <summary>
    /// Get player counts for each individual platform
    /// </summary>
    /// <returns>An instance of PlayerCountByPlatformResponse</returns>
    [HttpGet("playerCount/platform")]
    [ProducesResponseType(typeof(PlayerCountByPlatformResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlayerCountsByPlatform()
    {
        List<PlayerCountObject> platformList = new();
        foreach (Platform platform in platforms)
        {
            platformList.Add(new PlayerCountByPlatformObject
            {
                Platform = platform.ToString(),
                PlayerCount = await StatisticsHelper.RecentMatches(this.database, l => l.Platform == platform),
            });
        }

        PlayerCountByPlatformResponse response = new()
        {
            TotalPlayerCount = await StatisticsHelper.RecentMatches(this.database),
            Platforms = platformList,
        };

        return this.Ok(response);
    }

    /// <summary>
    /// Gets a list of online players 
    /// </summary>
    /// <returns>An instance of PlayerListResponse</returns>
    [HttpGet("players")]
    [ProducesResponseType(typeof(PlayerListResponse), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPlayerList()
    {
        List<PlayerListObject> players = await this.database.LastContacts.Where(l => TimeHelper.Timestamp - l.Timestamp < 300)
            .Select(l => new PlayerListObject
            {
                Username = l.User!.Username,
                Game = l.GameVersion.ToString(),
                Platform = l.Platform.ToString(),
            })
            .ToListAsync();

        PlayerListResponse response = new()
        {
            Players = players,
        };

        return this.Ok(response);
    }
}