#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
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
            /*
            if (matchData is CreateRoom createRoom)
            {
                if (createRoom.Slots.Count == 0) return this.BadRequest();
                if (createRoom.FirstSlot.Count != 2) return this.BadRequest();

                int slotType = createRoom.FirstSlot[0];
                int slotId = createRoom.FirstSlot[1];

                if (slotType == 1)
                {
                    Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slotId);
                    if (slot == null) return this.BadRequest();

                    slot.Plays++;
                    await this.database.SaveChangesAsync();
                }
            }
            */
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