#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Controllers.GameApi.Slots;

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

    [HttpPost("scoreboard/user/{id:int}")]
    public async Task<IActionResult> SubmitScore(int id, [FromQuery] bool lbp1 = false, [FromQuery] bool lbp2 = false, [FromQuery] bool lbp3 = false)
    {
        (User, GameToken)? userAndToken = await this.database.UserAndGameTokenFromRequest(this.Request);

        if (userAndToken == null) return this.StatusCode(403, "");

        // ReSharper disable once PossibleInvalidOperationException
        User user = userAndToken.Value.Item1;
        GameToken gameToken = userAndToken.Value.Item2;

        this.Request.Body.Position = 0;
        string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

        XmlSerializer serializer = new(typeof(Score));
        Score? score = (Score?)serializer.Deserialize(new StringReader(bodyString));
        if (score == null) return this.BadRequest();

        SanitizationHelper.SanitizeStringsInClass(score);

        score.SlotId = id;

        Slot? slot = this.database.Slots.FirstOrDefault(s => s.SlotId == score.SlotId);
        if (slot == null) return this.BadRequest();

        switch (gameToken.GameVersion)
        {
            case GameVersion.LittleBigPlanet1:
                slot.PlaysLBP1Complete++;
                break;
            case GameVersion.LittleBigPlanet2:
                slot.PlaysLBP2Complete++;
                break;
            case GameVersion.LittleBigPlanet3:
                slot.PlaysLBP3Complete++;
                break;
            case GameVersion.LittleBigPlanetVita:
                slot.PlaysLBPVitaComplete++;
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

        string myRanking = this.getScores(score.SlotId, score.Type, user, -1, 5, "scoreboardSegment");

        return this.Ok(myRanking);
    }

    [HttpGet("friendscores/user/{slotId:int}/{type:int}")]
    public IActionResult FriendScores(int slotId, int type)
        //=> await TopScores(slotId, type);
        => this.Ok(LbpSerializer.BlankElement("scores"));

    [HttpGet("topscores/user/{slotId:int}/{type:int}")]
    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    public async Task<IActionResult> TopScores(int slotId, int type, [FromQuery] int pageStart = -1, [FromQuery] int pageSize = 5)
    {
        // Get username
        User? user = await this.database.UserFromGameRequest(this.Request);

        if (user == null) return this.StatusCode(403, "");

        return this.Ok(this.getScores(slotId, type, user, pageStart, pageSize));
    }

    [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
    private string getScores(int slotId, int type, User user, int pageStart = -1, int pageSize = 5, string rootName = "scores")
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
        var myScore = rankedScores.Where(rs => rs.Score.PlayerIdCollection.Contains(user.Username)).OrderByDescending(rs => rs.Score.Points).FirstOrDefault();

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