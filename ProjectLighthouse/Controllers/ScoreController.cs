using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
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
            Score score = (Score)serializer.Deserialize(new StringReader(bodyString));
            if (score == null) return this.BadRequest();

            score.SlotId = id;

            this.database.Scores.Add(score);
            await this.database.SaveChangesAsync();

            return this.Ok();
        }
    }
}