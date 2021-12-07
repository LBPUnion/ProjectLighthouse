#nullable enable
using System;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers
{
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/plain")]
    public class ReviewController : ControllerBase
    {
        private readonly Database database;

        public ReviewController(Database database)
        {
            this.database = database;
        }

        // LBP1 rating
        [HttpPost("rate/user/{slotId}")]
        public async Task<IActionResult> Rate(int slotId, [FromQuery] int rating)
        {
            User? user = await this.database.UserFromGameRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            Slot? slot = await this.database.Slots.Include(s => s.Creator).Include(s => s.Location).FirstOrDefaultAsync(s => s.SlotId == slotId);
            if (slot == null) return this.StatusCode(403, "");

            RatedLevel? ratedLevel = await this.database.RatedLevels.FirstOrDefaultAsync(r => r.SlotId == slotId && r.UserId == user.UserId);
            if (ratedLevel == null)
            {
                ratedLevel = new RatedLevel();
                ratedLevel.SlotId = slotId;
                ratedLevel.UserId = user.UserId;
                ratedLevel.Rating = 0;
                this.database.RatedLevels.Add(ratedLevel);
            }

            ratedLevel.RatingLBP1 = Math.Max(Math.Min(5, rating), 0);

            await this.database.SaveChangesAsync();

            return this.Ok();
        }

        // LBP2 and beyond rating
        [HttpPost("dpadrate/user/{slotId:int}")]
        public async Task<IActionResult> DPadRate(int slotId, [FromQuery] int rating)
        {
            User? user = await this.database.UserFromGameRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            Slot? slot = await this.database.Slots.Include(s => s.Creator).Include(s => s.Location).FirstOrDefaultAsync(s => s.SlotId == slotId);
            if (slot == null) return this.StatusCode(403, "");

            RatedLevel? ratedLevel = await this.database.RatedLevels.FirstOrDefaultAsync(r => r.SlotId == slotId && r.UserId == user.UserId);
            if (ratedLevel == null)
            {
                ratedLevel = new RatedLevel();
                ratedLevel.SlotId = slotId;
                ratedLevel.UserId = user.UserId;
                ratedLevel.RatingLBP1 = 0;
                this.database.RatedLevels.Add(ratedLevel);
            }

            ratedLevel.Rating = Math.Max(Math.Min(1, rating), -1);

            await this.database.SaveChangesAsync();

            return this.Ok();
        }

    }
}