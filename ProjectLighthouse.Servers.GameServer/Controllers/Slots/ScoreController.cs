#nullable enable
using System.Diagnostics.CodeAnalysis;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.StorableLists.Stores;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class ScoreController : ControllerBase
{
    private readonly DatabaseContext database;

    public ScoreController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpPost("scoreboard/{slotType}/{id:int}")]
    [HttpPost("scoreboard/{slotType}/{id:int}/{childId:int}")]
    public async Task<IActionResult> SubmitScore(string slotType, int id, int childId)
    {
        GameTokenEntity token = this.GetToken();

        string username = await this.database.UsernameFromGameToken(token);

        if (SlotHelper.IsTypeInvalid(slotType))
        {
            Logger.Warn($"Rejecting score upload, slot type is invalid (slotType={slotType}, user={username})", LogArea.Score);
            return this.BadRequest();
        }

        GameScore? score = await this.DeserializeBody<GameScore>();
        if (score == null)
        {
            Logger.Warn($"Rejecting score upload, score is null (slotType={slotType}, slotId={id}, user={username})", LogArea.Score);
            return this.BadRequest();
        }

        //TODO versus levels having a ':' instead of ',' in score submit?

        if (score.PlayerIds.Length == 0)
        {
            Logger.Warn($"Rejecting score upload, there are 0 playerIds (slotType={slotType}, slotId={id}, user={username})", LogArea.Score);
            return this.BadRequest();
        }

        if (score.Points < 0)
        {
            Logger.Warn($"Rejecting score upload, points value is less than 0 (points={score.Points}, user={username})", LogArea.Score);
            return this.BadRequest();
        }

        // Score types:
        // 1-4: Co-op with the number representing the number of players
        // 5: leaderboard filtered by day (never uploaded with this id)
        // 6: leaderboard filtered by week (never uploaded either)
        // 7: Versus levels & leaderboard filtered by all time
        if (score.Type is > 4 or < 1 && score.Type != 7)
        {
            Logger.Warn($"Rejecting score upload, score type is out of bounds (type={score.Type}, user={username})", LogArea.Score);
            return this.BadRequest();
        }

        if (!score.PlayerIds.Contains(username))
        {
            string bodyString = await this.ReadBodyAsync();
            Logger.Warn("Rejecting score upload, requester username is not present in playerIds" +
                        $" (user={username}, playerIds={string.Join(",", score.PlayerIds)}, " +
                        $"gameVersion={token.GameVersion.ToPrettyString()}, type={score.Type}, id={id}, slotType={slotType}, body='{bodyString}')", LogArea.Score);
            return this.BadRequest();
        }

        SanitizationHelper.SanitizeStringsInClass(score);

        int slotId = id;

        if (slotType == "developer") slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

        SlotEntity? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null)
        {
            Logger.Warn($"Rejecting score upload, slot is null (slotId={slotId}, slotType={slotType}, reqId={id}, user={username})", LogArea.Score);
            return this.BadRequest();
        }

        switch (token.GameVersion)
        {
            case GameVersion.LittleBigPlanet1:
                slot.PlaysLBP1Complete++;
                break;
            case GameVersion.LittleBigPlanet2:
            case GameVersion.LittleBigPlanetVita:
                slot.PlaysLBP2Complete++;
                break;
            case GameVersion.LittleBigPlanet3:
                slot.PlaysLBP3Complete++;
                break;
            case GameVersion.LittleBigPlanetPSP:
            case GameVersion.Unknown:
            default:
                return this.BadRequest();
        }

        await this.database.SaveChangesAsync();

        ScoreEntity playerScore = new()
        {
            PlayerIdCollection = string.Join(',', score.PlayerIds),
            Type = score.Type,
            Points = score.Points,
            SlotId = slotId,
            ChildSlotId = childId,
        };

        IQueryable<ScoreEntity> existingScore = this.database.Scores.Where(s => s.SlotId == playerScore.SlotId)
            .Where(s => s.ChildSlotId == 0 || s.ChildSlotId == childId)
            .Where(s => s.PlayerIdCollection == playerScore.PlayerIdCollection)
            .Where(s => s.Type == playerScore.Type);
        if (await existingScore.AnyAsync())
        {
            ScoreEntity first = await existingScore.FirstAsync(s => s.SlotId == playerScore.SlotId);
            playerScore.ScoreId = first.ScoreId;
            playerScore.Points = Math.Max(first.Points, playerScore.Points);
            this.database.Entry(first).CurrentValues.SetValues(playerScore);
        }
        else
        {
            this.database.Scores.Add(playerScore);
        }

        await this.database.SaveChangesAsync();

        return this.Ok(this.getScores(new LeaderboardOptions
        {
            RootName = "scoreboardSegment",
            PageSize = 5,
            PageStart = -1,
            SlotId = slotId,
            ChildSlotId = childId,
            ScoreType = score.Type,
            TargetUsername = username,
            TargetPlayerIds = null,
        })); 
    }

    [HttpGet("friendscores/{slotType}/{slotId:int}/{type:int}")]
    [HttpGet("friendscores/{slotType}/{slotId:int}/{childId:int}/{type:int}")]
    public async Task<IActionResult> FriendScores(string slotType, int slotId, int? childId, int type, [FromQuery] int pageStart = -1, [FromQuery] int pageSize = 5)
    {
        GameTokenEntity token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        string username = await this.database.UsernameFromGameToken(token);

        if (SlotHelper.IsTypeInvalid(slotType)) return this.BadRequest();

        if (slotType == "developer") slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

        UserFriendData? store = UserFriendStore.GetUserFriendData(token.UserId);
        if (store == null) return this.Ok();

        List<string> friendNames = new()
        {
            username,
        };

        foreach (int friendId in store.FriendIds)
        {
            string? friendUsername = await this.database.Users.Where(u => u.UserId == friendId)
                .Select(u => u.Username)
                .FirstOrDefaultAsync();
            if (friendUsername != null) friendNames.Add(friendUsername);
        }

        return this.Ok(this.getScores(new LeaderboardOptions
        {
            RootName = "scores",
            PageSize = pageSize,
            PageStart = pageStart,
            SlotId = slotId,
            ChildSlotId = childId,
            ScoreType = type,
            TargetUsername = username,
            TargetPlayerIds = friendNames.ToArray(),
        })); 
    }

    [HttpGet("topscores/{slotType}/{slotId:int}/{type:int}")]
    [HttpGet("topscores/{slotType}/{slotId:int}/{childId:int}/{type:int}")]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task<IActionResult> TopScores(string slotType, int slotId, int? childId, int type, [FromQuery] int pageStart = -1, [FromQuery] int pageSize = 5)
    {
        GameTokenEntity token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        string username = await this.database.UsernameFromGameToken(token);

        if (SlotHelper.IsTypeInvalid(slotType)) return this.BadRequest();

        if (slotType == "developer") slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

        return this.Ok(this.getScores(new LeaderboardOptions
        {
            RootName = "scores",
            PageSize = pageSize,
            PageStart = pageStart,
            SlotId = slotId,
            ChildSlotId = childId,
            ScoreType = type,
            TargetUsername = username,
            TargetPlayerIds = null,
        }));
    }

    internal class LeaderboardOptions
    {
        public int SlotId { get; set; }
        public int ScoreType { get; set; }
        public string TargetUsername { get; set; } = "";
        public int PageStart { get; set; } = -1;
        public int PageSize { get; set; } = 5;
        public string RootName { get; set; } = "scores";
        public string[]? TargetPlayerIds;
        public int? ChildSlotId;
    }

    private ScoreboardResponse getScores(LeaderboardOptions options)
    {

        // This is hella ugly but it technically assigns the proper rank to a score
        // var needed for Anonymous type returned from SELECT
        var rankedScores = this.database.Scores.Where(s => s.SlotId == options.SlotId && s.Type == options.ScoreType)
            .Where(s => s.ChildSlotId == 0 || s.ChildSlotId == options.ChildSlotId)
            .AsEnumerable()
            .Where(s => options.TargetPlayerIds == null ||
                        options.TargetPlayerIds.Any(id => s.PlayerIdCollection.Split(",").Contains(id)))
            .OrderByDescending(s => s.Points)
            .ThenBy(s => s.ScoreId)
            .ToList()
            .Select((s, rank) => new
            {
                Score = s,
                Rank = rank + 1,
            })
            .ToList();


        // Find your score, since even if you aren't in the top list your score is pinned
        var myScore = rankedScores.Where(rs => rs.Score.PlayerIdCollection.Split(",").Contains(options.TargetUsername)).MaxBy(rs => rs.Score.Points);

        // Paginated viewing: if not requesting pageStart, get results around user
        var pagedScores = rankedScores.Skip(options.PageStart != -1 || myScore == null ? options.PageStart - 1 : myScore.Rank - 3).Take(Math.Min(options.PageSize, 30));

        List<GameScore> gameScores = pagedScores.Select(ps => GameScore.CreateFromEntity(ps.Score, ps.Rank)).ToList();

        return new ScoreboardResponse(options.RootName, gameScores, myScore?.Score.Points ?? 0, myScore?.Rank ?? 0, rankedScores.Count);
    }
}