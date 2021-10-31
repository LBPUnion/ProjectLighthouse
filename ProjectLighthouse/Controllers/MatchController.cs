#nullable enable
using System;
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

namespace LBPUnion.ProjectLighthouse.Controllers
{
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/xml")]
    public class MatchController : ControllerBase
    {
        private readonly Database database;

        public MatchController(Database database)
        {
            this.database = database;
        }

        [HttpPost("match")]
        [Produces("text/json")]
        public async Task<IActionResult> Match()
        {

            User? user = await this.database.UserFromRequest(this.Request);

            if (user == null) return this.StatusCode(403, "");

            #region Parse match data

            // Example POST /match: [UpdateMyPlayerData,["Player":"FireGamer9872"]]

            string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();
            if (bodyString.Contains("FindBestRoom"))
                return this.Ok
                (
                    "[{\"StatusCode\":200},{\"Players\":[{\"PlayerId\":\"literally1984\",\"matching_res\":0},{\"PlayerId\":\"jvyden\",\"matching_res\":1}],\"Slots\":[[5,0]],\"RoomState\":\"E_ROOM_IN_POD\",\"HostMood\":\"E_MOOD_EVERYONE\",\"LevelCompletionEstimate\":0,\"PassedNoJoinPoint\":0,\"MoveConnected\":false,\"Location\":[\"127.0.0.1\"],\"BuildVersion\":289,\"Language\":1,\"FirstSeenTimestamp\":1427331263756,\"LastSeenTimestamp\":1635112546000,\"GameId\":1,\"NatType\":2,\"Friends\":[],\"Blocked\":[],\"RecentlyLeft\":[],\"FailedJoin\":[]}]"
                );

            if (bodyString[0] != '[') return this.BadRequest();

            IMatchData? matchData;
            try
            {
                matchData = MatchHelper.Deserialize(bodyString);
            }
            catch(Exception e)
            {
                Logger.Log("Exception while parsing MatchData: " + e);
                Logger.Log("Data: " + bodyString);

                return this.BadRequest();
            }

            if (matchData == null) return this.BadRequest();

            #endregion

            #region Update LastMatch

            LastMatch? lastMatch = await this.database.LastMatches.Where(l => l.UserId == user.UserId).FirstOrDefaultAsync();

            // below makes it not look like trash
            // ReSharper disable once ConvertIfStatementToNullCoalescingExpression
            if (lastMatch == null)
            {
                lastMatch = new LastMatch
                {
                    UserId = user.UserId,
                };
                this.database.LastMatches.Add(lastMatch);
            }

            lastMatch.Timestamp = TimestampHelper.Timestamp;

            await this.database.SaveChangesAsync();

            #endregion

            return this.Ok("[{\"StatusCode\":200}]");
        }
    }
}