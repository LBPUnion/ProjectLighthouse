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

        [HttpPost("dpadrate/user/{slotId}")]
        public async Task<IActionResult> DPadRate(int slotId, [FromQuery] int rating)
        {
            User? user = await this.database.UserFromRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slotId);
            if (slot == null) return this.StatusCode(403, "");

            RatedLevel? ratedLevel = await this.database.RatedLevels.FirstOrDefaultAsync(r => r.SlotId == slotId && r.UserId == user.UserId);
            if (ratedLevel == null)
            {
                ratedLevel = new();
                this.database.RatedLevels.Add(ratedLevel);
            }
            ratedLevel.SlotId = slotId;
            ratedLevel.UserId = user.UserId;
            ratedLevel.Rating = rating;
            // Unsupported: ratedLevel.LBP1Rating

            await this.database.SaveChangesAsync();

            return this.Ok();
        }
    }
}