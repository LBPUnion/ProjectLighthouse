#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
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
        [Produces("text/plain")]
        public async Task<IActionResult> Match()
        {
            (User, Token)? userAndToken = await this.database.UserAndTokenFromRequest(this.Request);

            if (userAndToken == null) return this.StatusCode(403, "");

            // ReSharper disable once PossibleInvalidOperationException
            User user = userAndToken.Value.Item1;
            Token token = userAndToken.Value.Item2;

            #region Parse match data

            // Example POST /match: [UpdateMyPlayerData,["Player":"FireGamer9872"]]

            string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

            if (bodyString.Length == 0 || bodyString[0] != '[') return this.BadRequest();

            IMatchData? matchData;
            try
            {
                matchData = MatchHelper.Deserialize(bodyString);
            }
            catch(Exception e)
            {
                Logger.Log("Exception while parsing MatchData: " + e, LoggerLevelMatch.Instance);
                Logger.Log("Data: " + bodyString, LoggerLevelMatch.Instance);

                return this.BadRequest();
            }

            if (matchData == null) return this.BadRequest();

            #endregion

            #region Process match data

            if (matchData is UpdateMyPlayerData) MatchHelper.SetUserLocation(user.UserId, token.UserLocation);

            if (matchData is FindBestRoom && MatchHelper.UserLocations.Count > 1)
            {
                foreach ((int id, string? location) in MatchHelper.UserLocations)
                {
                    if (id == user.UserId) continue;
                    if (location == null) continue;
                    if (MatchHelper.DidUserRecentlyDiveInWith(user.UserId, id)) continue;

                    User? otherUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
                    if (otherUser == null) continue;

                    FindBestRoomResponse response = MatchHelper.FindBestRoomResponse(user.Username, otherUser.Username, token.UserLocation, location);

                    string serialized = JsonSerializer.Serialize(response, typeof(FindBestRoomResponse));

                    MatchHelper.AddUserRecentlyDivedIn(user.UserId, id);

                    return new ObjectResult($"[{{\"StatusCode\":200}},{serialized}]");
                }
            }

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