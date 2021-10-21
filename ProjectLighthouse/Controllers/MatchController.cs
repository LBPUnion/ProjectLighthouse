#nullable enable
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectLighthouse.Helpers;
using ProjectLighthouse.Types;
using ProjectLighthouse.Types.Profiles;

namespace ProjectLighthouse.Controllers {
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
            User? user = await this.database.UserFromRequest(this.Request);

            if(user == null) return this.StatusCode(403, "");
            LastMatch? lastMatch = await this.database.LastMatches
                .Where(l => l.UserId == user.UserId).FirstOrDefaultAsync();

            // below makes it not look like trash
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if(lastMatch == null) {
                lastMatch = new LastMatch {
                    UserId = user.UserId,
                };
                this.database.LastMatches.Add(lastMatch);
            }

            lastMatch.Timestamp = TimestampHelper.Timestamp;

            await this.database.SaveChangesAsync();
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