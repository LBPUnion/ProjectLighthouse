#nullable enable
using System.Diagnostics.CodeAnalysis;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
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

    private string[] getFriendUsernames(int userId, string username)
    {
        UserFriendData? store = UserFriendStore.GetUserFriendData(userId);
        if (store == null) return new[] { username, };

        List<string> friendNames = new()
        {
            username,
        };

        // ReSharper disable once ForeachCanBeConvertedToQueryUsingAnotherGetEnumerator
        foreach (int friendId in store.FriendIds)
        {
            string? friendUsername = this.database.Users.Where(u => u.UserId == friendId)
                .Select(u => u.Username)
                .FirstOrDefault();
            if (friendUsername != null) friendNames.Add(friendUsername);
        }

        return friendNames.ToArray();
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

        // Workaround for parsing player ids of versus levels
        if (score.PlayerIds.Length == 1)
        {
            char[] delimiters = { ':', ',', };
            foreach (char delimiter in delimiters)
            {
                score.PlayerIds = score.PlayerIds[0].Split(delimiter, StringSplitOptions.RemoveEmptyEntries);
            }
                
        }

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
                        $" (user='{username}', playerIds='{string.Join(",", score.PlayerIds)}' playerIds.Length={score.PlayerIds.Length}, " +
                        $"gameVersion={token.GameVersion.ToPrettyString()}, type={score.Type}, id={id}, slotType={slotType}, body='{bodyString}')",
                LogArea.Score);
            return this.BadRequest();
        }

        int slotId = id;

        if (slotType == "developer") slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

        SlotEntity? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slotId);
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

        string playerIdCollection = string.Join(',', score.PlayerIds);

        ScoreEntity? existingScore = await this.database.Scores.Where(s => s.SlotId == slot.SlotId)
            .Where(s => s.ChildSlotId == 0 || s.ChildSlotId == childId)
            .Where(s => s.PlayerIdCollection == playerIdCollection)
            .Where(s => s.Type == score.Type)
            .FirstOrDefaultAsync();
        if (existingScore != null)
        {
            existingScore.Points = Math.Max(existingScore.Points, score.Points);
        }
        else
        {
            ScoreEntity playerScore = new()
            {
                PlayerIdCollection = playerIdCollection,
                Type = score.Type,
                Points = score.Points,
                SlotId = slotId,
                ChildSlotId = childId,
            };
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

    [HttpGet("scoreboard/{slotType}/{id:int}")]
    [HttpPost("scoreboard/friends/{slotType}/{id:int}")]
    public async Task<IActionResult> Lbp1Leaderboards(string slotType, int id)
    {
        GameTokenEntity token = this.GetToken();

        string username = await this.database.UsernameFromGameToken(token);

        if (slotType == "developer") id = await SlotHelper.GetPlaceholderSlotId(this.database, id, SlotType.Developer);

        LeaderboardOptions options = new()
        {
            PageSize = 10,
            PageStart = 1,
            ScoreType = -1,
            SlotId = id,
            TargetUsername = username,
            RootName = "scoreboardSegment",
        };
        if (!HttpMethods.IsPost(this.Request.Method))
        {
            List<PlayerScoreboardResponse> scoreboardResponses = new();
            for (int i = 1; i <= 4; i++)
            {
                options.ScoreType = i;
                ScoreboardResponse response = this.getScores(options);
                scoreboardResponses.Add(new PlayerScoreboardResponse(response.Scores, i));
            } 
            return this.Ok(new MultiScoreboardResponse(scoreboardResponses));
        }

        GameScore? score = await this.DeserializeBody<GameScore>();
        if (score == null) return this.BadRequest();
        options.ScoreType = score.Type;
        options.TargetPlayerIds = this.getFriendUsernames(token.UserId, username);

        return this.Ok(this.getScores(options));
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

        string[] friendIds = this.getFriendUsernames(token.UserId, username);

        return this.Ok(this.getScores(new LeaderboardOptions
        {
            RootName = "scores",
            PageSize = pageSize,
            PageStart = pageStart,
            SlotId = slotId,
            ChildSlotId = childId,
            ScoreType = type,
            TargetUsername = username,
            TargetPlayerIds = friendIds,
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

    private class LeaderboardOptions
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
        var rankedScores = this.database.Scores.Where(s => s.SlotId == options.SlotId)
            .Where(s => options.ScoreType == -1 || s.Type == options.ScoreType)
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

        List<GameScore> gameScores = pagedScores.ToSerializableList(ps => GameScore.CreateFromEntity(ps.Score, ps.Rank));

        return new ScoreboardResponse(options.RootName, gameScores, rankedScores.Count, myScore?.Score.Points ?? 0, myScore?.Rank ?? 0);
    }
}