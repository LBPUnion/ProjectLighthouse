#nullable enable
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Match;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/xml")]
    public class MatchController : ControllerBase {
        private readonly Database database;
        public MatchController(Database database) {
            this.database = database;
        }

        [HttpPost("match")]
        [Produces("text/json")]
        public async Task<IActionResult> Match() {
//            User? user = await this.database.UserFromRequest(this.Request);
//
//            if(user == null) return this.StatusCode(403, "");

            #region Parse match data
            // Example POST /match: [UpdateMyPlayerData,["Player":"FireGamer9872"]]
            
            string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();
            if(bodyString[0] != '[') return this.BadRequest();

            string matchType = "";

            int i = 1;
            while(true) {
                if(bodyString[i] == ',') break;

                matchType += bodyString[i];
                i++;
            }

            string matchString = string.Concat(bodyString.Skip(matchType.Length + 2).SkipLast(1));
            #endregion

            #region Update LastMatch
//            LastMatch? lastMatch = await this.database.LastMatches
//                .Where(l => l.UserId == user.UserId).FirstOrDefaultAsync();
//
//            // below makes it not look like trash
//            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
//            if(lastMatch == null) {
//                lastMatch = new LastMatch {
//                    UserId = user.UserId,
//                };
//                this.database.LastMatches.Add(lastMatch);
//            }
//
//            lastMatch.Timestamp = TimestampHelper.Timestamp;
//
//            await this.database.SaveChangesAsync();
            #endregion
            
            return this.Ok("[{\"StatusCode\":200}]");
        }

        [HttpGet("playersInPodCount")]
        [HttpGet("totalPlayerCount")]
        public async Task<IActionResult> TotalPlayerCount() {
            int recentMatches = await this.database.LastMatches
                .Where(l => TimestampHelper.Timestamp - l.Timestamp < 60)
                .CountAsync();

            return this.Ok(recentMatches.ToString());
        }
    }
}