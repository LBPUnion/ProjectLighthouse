#nullable enable
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Controllers
{
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/plain")]
    public class ScoreController : ControllerBase
    {
        private readonly Database database;

        public ScoreController(Database database)
        {
            this.database = database;
        }

        [HttpPost("scoreboard/user/{id:int}")]
        public async Task<IActionResult> SubmitScore(int id)
        {
            this.Request.Body.Position = 0;
            string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

            XmlSerializer serializer = new(typeof(Score));
            Score? score = (Score?)serializer.Deserialize(new StringReader(bodyString));
            if (score == null) return this.BadRequest();

            score.SlotId = id;

            IQueryable<Score> existingScore = this.database.Scores.Where(s => s.SlotId == score.SlotId && s.PlayerIdCollection == score.PlayerIdCollection);

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

            return this.Ok();
        }

        [HttpGet("topscores/user/{slotId:int}/{type:int}")]
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public async Task<IActionResult> TopScores(int slotId, int type, [FromQuery] int pageStart, [FromQuery] int pageSize)
        {
            // Get username
            User? user = await this.database.UserFromRequest(this.Request);

            if (user == null) return this.StatusCode(403, "");

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
            var myScore = rankedScores.Where
                    (rs => rs.Score.PlayerIdCollection.Contains(user.Username))
                .OrderByDescending(rs => rs.Score.Points)
                .FirstOrDefault();

            // Paginated viewing
            var pagedScores = rankedScores.Skip(pageStart - 1).Take(Math.Min(pageSize, 30));

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
            if (myScore == null)
            {
                res = LbpSerializer.StringElement("scores", serializedScores);
            }
            else
            {
                res = LbpSerializer.TaggedStringElement
                (
                    "scores",
                    serializedScores,
                    new Dictionary<string, object>()
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
            }

            return this.Ok(res);
        }
    }
}