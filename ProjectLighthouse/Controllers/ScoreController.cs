using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Serialization;
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
            Score score = (Score)serializer.Deserialize(new StringReader(bodyString));
            if (score == null) return this.BadRequest();

            score.SlotId = id;

            IQueryable<Score> existingScore = this.database.Scores.Where(s => s.PlayerIdCollection == score.PlayerIdCollection);

            if (existingScore.Any())
            {
                Score first = existingScore.FirstOrDefault(s => s.SlotId == score.SlotId);
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
        public async Task<IActionResult> TopScores(int slotId, int type, [FromQuery] int pageStart, [FromQuery] int pageSize)
        {
            // Get username
            User user = await this.database.UserFromRequest(this.Request);

            // Find your score, even if you aren't in the top list
            List<Score> myScore = this.database.Scores
                .Where(s => s.SlotId == slotId && s.Type == type && s.PlayerIdCollection.Contains(user.Username)).ToList();

            //Split this out from pagination, so we can count totalNumScores below
            IQueryable<Score> allScoresOfType = this.database.Scores
                .Where(s => s.SlotId == slotId && s.Type == type);

            //Paginated viewing
            IQueryable<Score> pagedScores = allScoresOfType
                .OrderByDescending(s => s.Points)
                .Skip(pageStart - 1)
                .Take(Math.Min(pageSize, 30));

            //Calculate rank and serialize top scores
            int rank = 1;
            string serializedScores = Enumerable.Aggregate(pagedScores, string.Empty, (current, score) => {
                score.Rank = (pageStart - 1) * pageSize + rank++;
                return current + score.Serialize();
            });

            string res = LbpSerializer.TaggedStringElement("scores", serializedScores, new Dictionary<string, object>() {
                {"yourScore",  myScore.OrderByDescending(score => score.Points).FirstOrDefault()},
                {"yourRank", 0 }, // afaik, this is unused
                {"totalNumScores", allScoresOfType.Count() } // This is shown as "Ranked 1/x" in the side menu if you have the global highscore
            });

            return this.Ok(res);
        }
    }
}