#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.StorableLists.Stores;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class ScoreController : ControllerBase
{
    private readonly Database database;

    public ScoreController(Database database)
    {
        this.database = database;
    }

    [HttpPost("scoreboard/{slotType}/{id:int}")]
    [HttpPost("scoreboard/{slotType}/{id:int}/{childId:int}")]
    public async Task<IActionResult> SubmitScore(string slotType, int id, int? childId, [FromQuery] bool lbp1 = false, [FromQuery] bool lbp2 = false, [FromQuery] bool lbp3 = false)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        string username = await this.database.UsernameFromGameToken(token);

        if (SlotHelper.IsTypeInvalid(slotType))
        {
            Logger.Warn($"Rejecting score upload, slot type is invalid (slotType={slotType}, user={username})", LogArea.Score);
            return this.BadRequest();
        }

        this.Request.Body.Position = 0;
        string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

        XmlSerializer serializer = new(typeof(Score));
        Score? score = (Score?)serializer.Deserialize(new StringReader(bodyString));
        if (score == null)
        {
            Logger.Warn($"Rejecting score upload, score is null (slotType={slotType}, slotId={id}, user={username})", LogArea.Score);
            return this.BadRequest();
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

        SanitizationHelper.SanitizeStringsInClass(score);

        if (slotType == "developer") id = await SlotHelper.GetPlaceholderSlotId(this.database, id, SlotType.Developer);

        score.SlotId = id;
        score.ChildSlotId = childId;

        Slot? slot = this.database.Slots.FirstOrDefault(s => s.SlotId == score.SlotId);
        if (slot == null)
        {
            Logger.Warn($"Rejecting score upload, slot is null (slotId={score.SlotId}, slotType={slotType}, reqId={id}, user={username})", LogArea.Score);
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
            case GameVersion.LittleBigPlanetPSP: break;
            case GameVersion.Unknown: break;
            default: throw new ArgumentOutOfRangeException();
        }

        // Submit scores from all players in lobby
        foreach (string player in score.PlayerIds)
        {
            List<string> players = new();
            players.Add(player); // make sure this player is first
            players.AddRange(score.PlayerIds.Where(p => p != player));

            Score playerScore = new()
            {
                PlayerIdCollection = string.Join(',', players),
                Type = score.Type,
                Points = score.Points,
                SlotId = score.SlotId,
                ChildSlotId = score.ChildSlotId,
            };

            IQueryable<Score> existingScore = this.database.Scores.Where(s => s.SlotId == playerScore.SlotId)
            .Where(s => s.ChildSlotId == 0 || s.ChildSlotId == childId)
            .Where(s => s.PlayerIdCollection == playerScore.PlayerIdCollection)
            .Where(s => s.Type == playerScore.Type);
            if (existingScore.Any())
            {
                Score first = existingScore.First(s => s.SlotId == playerScore.SlotId);
                playerScore.ScoreId = first.ScoreId;
                playerScore.Points = Math.Max(first.Points, playerScore.Points);
                this.database.Entry(first).CurrentValues.SetValues(playerScore);
            }
            else
            {
                this.database.Scores.Add(playerScore);
            }
        }

        await this.database.SaveChangesAsync();

        string myRanking = this.getScores(score.SlotId, score.Type, username, -1, 5, "scoreboardSegment", childId: score.ChildSlotId);

        return this.Ok(myRanking);
    }

    [HttpGet("friendscores/{slotType}/{slotId:int}/{type:int}")]
    [HttpGet("friendscores/{slotType}/{slotId:int}/{childId:int}/{type:int}")]
    public async Task<IActionResult> FriendScores(string slotType, int slotId, int? childId, int type, [FromQuery] int pageStart = -1, [FromQuery] int pageSize = 5)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        if (pageSize <= 0) return this.BadRequest();

        string username = await this.database.UsernameFromGameToken(token);

        if (SlotHelper.IsTypeInvalid(slotType)) return this.BadRequest();

        if (slotType == "developer") slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

        UserFriendData? store = UserFriendStore.GetUserFriendData(token.UserId);
        if (store == null) return this.Ok();

        List<string> friendNames = new();

        foreach (int friendId in store.FriendIds)
        {
            string? friendUsername = await this.database.Users.Where(u => u.UserId == friendId)
                .Select(u => u.Username)
                .FirstOrDefaultAsync();
            if (friendUsername != null) friendNames.Add(friendUsername);
        }

        return this.Ok(this.getScores(slotId, type, username, pageStart, pageSize, "scores", friendNames.ToArray(), childId)); 
    }

    [HttpGet("topscores/{slotType}/{slotId:int}/{type:int}")]
    [HttpGet("topscores/{slotType}/{slotId:int}/{childId:int}/{type:int}")]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task<IActionResult> TopScores(string slotType, int slotId, int? childId, int type, [FromQuery] int pageStart = -1, [FromQuery] int pageSize = 5)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        if (pageSize <= 0) return this.BadRequest();

        string username = await this.database.UsernameFromGameToken(token);

        if (SlotHelper.IsTypeInvalid(slotType)) return this.BadRequest();

        if (slotType == "developer") slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

        return this.Ok(this.getScores(slotId, type, username, pageStart, pageSize, childId: childId));
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    private string getScores
    (
        int slotId,
        int type,
        string username,
        int pageStart = -1,
        int pageSize = 5,
        string rootName = "scores",
        string[]? playerIds = null,
        int? childId = 0
    )
    {

        // This is hella ugly but it technically assigns the proper rank to a score
        // var needed for Anonymous type returned from SELECT
        var rankedScores = this.database.Scores
            .Where(s => s.SlotId == slotId && s.Type == type)
            .Where(s => s.ChildSlotId == 0 || s.ChildSlotId == childId)
            .Where(s => playerIds == null || playerIds.Any(id => s.PlayerIdCollection.Contains(id)))
            .AsEnumerable()
            .OrderByDescending(s => s.Points)
            .ThenBy(s => s.ScoreId)
            .ToList()
            .Select
            (
                (s, rank) => new
                {
                    Score = s,
                    Rank = rank + 1,
                }
            );

        // Find your score, since even if you aren't in the top list your score is pinned
        var myScore = rankedScores.Where(rs => rs.Score.PlayerIdCollection.Contains(username)).MaxBy(rs => rs.Score.Points);

        // Paginated viewing: if not requesting pageStart, get results around user
        var pagedScores = rankedScores.Skip(pageStart != -1 || myScore == null ? pageStart - 1 : myScore.Rank - 3).Take(Math.Min(pageSize, 30));

        string serializedScores = pagedScores.Aggregate
        (
            string.Empty,
            (current, rs) =>
            {
                rs.Score.Rank = rs.Rank;
                return current + rs.Score.Serialize();
            }
        );

        string res;
        if (myScore == null) res = LbpSerializer.StringElement(rootName, serializedScores);
        else
            res = LbpSerializer.TaggedStringElement
            (
                rootName,
                serializedScores,
                new Dictionary<string, object>
                {
                    {
                        "yourScore", myScore.Score.Points
                    },
                    {
                        "yourRank", myScore.Rank
                    }, //This is the numerator of your position globally in the side menu.
                    {
                        "totalNumScores", rankedScores.Count()
                    }, // This is the denominator of your position globally in the side menu.
                }
            );

        return res;
    }
}