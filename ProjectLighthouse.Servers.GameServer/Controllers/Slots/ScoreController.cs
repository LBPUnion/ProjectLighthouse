#nullable enable
using System.Diagnostics.CodeAnalysis;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<IActionResult> SubmitScore(string slotType, int id, [FromQuery] bool lbp1 = false, [FromQuery] bool lbp2 = false, [FromQuery] bool lbp3 = false)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        string username = await this.database.UsernameFromGameToken(token);

        if (SlotHelper.IsTypeInvalid(slotType)) return this.BadRequest();

        this.Request.Body.Position = 0;
        string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

        XmlSerializer serializer = new(typeof(Score));
        Score? score = (Score?)serializer.Deserialize(new StringReader(bodyString));
        if (score == null) return this.BadRequest();

        SanitizationHelper.SanitizeStringsInClass(score);

        if (slotType == "developer") id = await SlotHelper.GetPlaceholderSlotId(this.database, id, SlotType.Developer);

        score.SlotId = id;

        Slot? slot = this.database.Slots.FirstOrDefault(s => s.SlotId == score.SlotId);
        if (slot == null) return this.BadRequest();

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
        }

        IQueryable<Score> existingScore = this.database.Scores.Where(s => s.SlotId == score.SlotId)
            .Where(s => s.PlayerIdCollection == score.PlayerIdCollection)
            .Where(s => s.Type == score.Type);

        if (existingScore.Any())
        {
            Score first = existingScore.First(s => s.SlotId == score.SlotId);
            score.ScoreId = first.ScoreId;
            score.Points = Math.Max(first.Points, score.Points);
            this.database.Entry(first).CurrentValues.SetValues(score);
        }
        else
        {
            this.database.Scores.Add(score);
        }

        await this.database.SaveChangesAsync();

        string myRanking = this.getScores(score.SlotId, score.Type, username, -1, 5, "scoreboardSegment");

        return this.Ok(myRanking);
    }

    [HttpGet("friendscores/user/{slotId:int}/{type:int}")]
    public IActionResult FriendScores(int slotId, int type)
        //=> await TopScores(slotId, type);
        => this.Ok(LbpSerializer.BlankElement("scores"));

    [HttpGet("topscores/{slotType}/{slotId:int}/{type:int}")]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task<IActionResult> TopScores(string slotType, int slotId, int type, [FromQuery] int pageStart = -1, [FromQuery] int pageSize = 5)
    {
        // Get username
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        if (pageSize <= 0) return this.BadRequest();

        string username = await this.database.UsernameFromGameToken(token);

        if (SlotHelper.IsTypeInvalid(slotType)) return this.BadRequest();

        if (slotType == "developer") slotId = await SlotHelper.GetPlaceholderSlotId(this.database, slotId, SlotType.Developer);

        return this.Ok(this.getScores(slotId, type, username, pageStart, pageSize));
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    private string getScores
    (
        int slotId,
        int type,
        string username,
        int pageStart = -1,
        int pageSize = 5,
        string rootName = "scores"
    )
    {

        // This is hella ugly but it technically assigns the proper rank to a score
        // var needed for Anonymous type returned from SELECT
        var rankedScores = this.database.Scores.Where(s => s.SlotId == slotId && s.Type == type)
            .OrderByDescending(s => s.Points)
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