#nullable enable
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
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
                Logger.Log("Exception while parsing MatchData: " + e);
                Logger.Log("Data: " + bodyString);

                return this.BadRequest();
            }

            if (matchData == null) return this.BadRequest();

            #endregion

            #region Process match data

            if (matchData is UpdateMyPlayerData)
            {
                if (MatchHelper.UserLocations.TryGetValue(user.UserId, out string? _)) MatchHelper.UserLocations.Remove(user.UserId);
                MatchHelper.UserLocations.Add(user.UserId, token.UserLocation);
            }

            if (matchData is FindBestRoom findBestRoom)
            {
                if (MatchHelper.UserLocations.Count > 1)
                {
                    foreach ((int id, string? location) in MatchHelper.UserLocations)
                    {
                        if (id == user.UserId) continue;
                        if (location == null) continue;

                        User? otherUser = await this.database.Users.FirstOrDefaultAsync(u => u.UserId == id);
                        if (otherUser == null) continue;

                        FindBestRoomResponse response = new()
                        {
                            Players = new List<Player>
                            {
                                new()
                                {
                                    MatchingRes = 0,
                                    PlayerId = otherUser.Username,
                                },
                                new()
                                {
                                    MatchingRes = 1,
                                    PlayerId = user.Username,
                                },
                            },
                            Locations = new List<string>
                            {
                                location,
                                token.UserLocation,
                            },
                            Slots = new List<List<int>>
                            {
                                new()
                                {
                                    5,
                                    0,
                                },
                            },
                        };

                        string serialized = JsonSerializer.Serialize(response, typeof(FindBestRoomResponse));

                        return new ObjectResult($"[{{\"StatusCode\":200}},{serialized}]");
                    }
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