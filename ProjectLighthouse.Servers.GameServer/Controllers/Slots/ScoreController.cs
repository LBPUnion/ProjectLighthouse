#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.StorableLists.Stores;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
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

    private static int[] GetFriendIds(int userId)
    {
        UserFriendData? store = UserFriendStore.GetUserFriendData(userId);
        List<int>? friendIds = store?.FriendIds;
        friendIds ??= new List<int>();
        friendIds.Add(userId);

        return friendIds.Distinct().ToArray();
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
            score.PlayerIds = score.PlayerIds[0].Split(delimiters).Distinct().ToArray();
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

        ScoreEntity? existingScore = await this.database.Scores.Where(s => s.SlotId == slot.SlotId)
            .Where(s => s.ChildSlotId == 0 || s.ChildSlotId == childId)
            .Where(s => s.UserId == token.UserId)
            .Where(s => s.Type == score.Type)
            .FirstOrDefaultAsync();

        if (existingScore == null)
        {
            existingScore = new ScoreEntity
            {
                UserId = token.UserId,
                Type = score.Type,
                Points = score.Points,
                SlotId = slotId,
                ChildSlotId = childId,
                Timestamp = TimeHelper.TimestampMillis,
            };
            this.database.Scores.Add(existingScore);
        }

        bool personalBest = score.Points > existingScore.Points;
        
        if (personalBest)
        {
            existingScore.Points = score.Points;
            existingScore.Timestamp = TimeHelper.TimestampMillis;
        }

        await this.database.SaveChangesAsync();

        ScoreboardResponse scores = await this.GetScores(new LeaderboardOptions
        {
            RootName = "scoreboardSegment",
            PageSize = 5,
            PageStart = -1,
            SlotId = slotId,
            ChildSlotId = childId,
            ScoreType = score.Type,
            TargetUser = token.UserId,
            TargetPlayerIds = null,
        });

        // if this is a PB, singleplayer, at the top of the leaderboard (not scores.YourRank==1 because it might be tied), and there is at least one other score,
        // send a notification to the user with the previous highscore
        if (personalBest && score.Type == 1 && scores.Scores[0].UserId == token.UserId && scores.Total > 1)
        {
            GameScore? second = scores.Scores[1];
            UserEntity? user = await this.database.UserFromGameToken(token);

            await this.database.SendNotification(second.UserId, $"{user?.InfoXml} beat your highscore (<em>{second.Points}</em>) on {slot.InfoXml} with a score of <em>{score.Points}</em>.", false);
        }

        return this.Ok(scores); 
    }

    [HttpGet("scoreboard/{slotType}/{id:int}")]
    [HttpPost("scoreboard/friends/{slotType}/{id:int}")]
    public async Task<IActionResult> Lbp1Leaderboards(string slotType, int id)
    {
        GameTokenEntity token = this.GetToken();

        if (slotType == "developer") id = await SlotHelper.GetPlaceholderSlotId(this.database, id, SlotType.Developer);

        LeaderboardOptions options = new()
        {
            PageSize = 10,
            PageStart = 1,
            ScoreType = -1,
            SlotId = id,
            TargetUser = token.UserId,
            RootName = "scoreboardSegment",
        };
        if (!HttpMethods.IsPost(this.Request.Method))
        {
            List<PlayerScoreboardResponse> scoreboardResponses = new();
            for (int i = 1; i <= 4; i++)
            {
                options.ScoreType = i;
                ScoreboardResponse response = await this.GetScores(options);
                scoreboardResponses.Add(new PlayerScoreboardResponse(response.Scores, i));
            } 
            return this.Ok(new MultiScoreboardResponse(scoreboardResponses));
        }

        GameScore? score = await this.DeserializeBody<GameScore>();
        if (score == null) return this.BadRequest();
        options.ScoreType = score.Type;
        options.TargetPlayerIds = GetFriendIds(token.UserId);

        return this.Ok(await this.GetScores(options));
    }

    [HttpGet("friendscores/{slotType}/{slotId:int}/{type:int}")]
    [HttpGet("friendscores/{slotType}/{slotId:int}/{childId:int}/{type:int}")]
    public async Task<IActionResult> FriendScores(string slotType, int slotId, int? childId, int type, [FromQuery] int pageStart = -1, [FromQuery] int pageSize = 5)
    {
        GameTokenEntity token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        if (SlotHelper.IsTypeInvalid(slotType)) return this.BadRequest();

        if (slotType == "developer") slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

        int[] friendIds = GetFriendIds(token.UserId);

        return this.Ok(await this.GetScores(new LeaderboardOptions
        {
            RootName = "scores",
            PageSize = pageSize,
            PageStart = pageStart,
            SlotId = slotId,
            ChildSlotId = childId,
            ScoreType = type,
            TargetUser = token.UserId,
            TargetPlayerIds = friendIds,
        })); 
    }

    [HttpGet("topscores/{slotType}/{slotId:int}/{type:int}")]
    [HttpGet("topscores/{slotType}/{slotId:int}/{childId:int}/{type:int}")]
    public async Task<IActionResult> TopScores(string slotType, int slotId, int? childId, int type, [FromQuery] int pageStart = -1, [FromQuery] int pageSize = 5)
    {
        GameTokenEntity token = this.GetToken();

        if (pageSize <= 0) return this.BadRequest();

        if (SlotHelper.IsTypeInvalid(slotType)) return this.BadRequest();

        if (slotType == "developer") slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

        return this.Ok(await this.GetScores(new LeaderboardOptions
        {
            RootName = "scores",
            PageSize = pageSize,
            PageStart = pageStart,
            SlotId = slotId,
            ChildSlotId = childId,
            ScoreType = type,
            TargetUser = token.UserId,
            TargetPlayerIds = null,
        }));
    }

    private class LeaderboardOptions
    {
        public int SlotId { get; init; }
        public int ScoreType { get; set; }
        public int TargetUser { get; init; }
        public int PageStart { get; init; } = -1;
        public int PageSize { get; init; } = 5;
        public string RootName { get; init; } = "scores";
        public int[]? TargetPlayerIds;
        public int? ChildSlotId;
    }

    private async Task<ScoreboardResponse> GetScores(LeaderboardOptions options)
    {
        IQueryable<ScoreEntity> scoreQuery = this.database.Scores.Where(s => s.SlotId == options.SlotId)
            .Where(s => options.ScoreType == -1 || s.Type == options.ScoreType)
            .Where(s => s.ChildSlotId == 0 || s.ChildSlotId == options.ChildSlotId)
            .Where(s => options.TargetPlayerIds == null || options.TargetPlayerIds.Contains(s.UserId));

        // First find if you have a score on a level to find scores around it
        var myScore = await scoreQuery.Where(s => s.UserId == options.TargetUser)
            .Select(s => new
            {
                Score = s,
                Rank = scoreQuery.Count(s2 => s2.Points > s.Points) + 1,
            }).FirstOrDefaultAsync();

        int skipAmt = options.PageStart != -1 || myScore == null ? options.PageStart - 1 : myScore.Rank - 3;

        var rankedScores = scoreQuery.OrderByDescending(s => s.Points)
            .ThenBy(s => s.Timestamp)
            .ThenBy(s => s.ScoreId)
            .Skip(Math.Max(0, skipAmt))
            .Take(Math.Min(options.PageSize, 30))
            .Select(s => new
            {
               Score = s,
               Rank = scoreQuery.Count(s2 => s2.Points > s.Points) + 1,
            })
            .ToList();

        int totalScores = scoreQuery.Count();

        List<GameScore> gameScores = rankedScores.ToSerializableList(ps => GameScore.CreateFromEntity(ps.Score, ps.Rank));

        return new ScoreboardResponse(options.RootName, gameScores, totalScores, myScore?.Score.Points ?? 0, myScore?.Rank ?? 0);
    }
}