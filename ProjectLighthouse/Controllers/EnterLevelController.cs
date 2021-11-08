#nullable enable
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers
{
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/enterLevel")]
//    [Produces("text/plain")]
    public class EnterLevelController : ControllerBase
    {
        private readonly Database database;

        public EnterLevelController(Database database)
        {
            this.database = database;
        }

        // Only used in LBP1
        [HttpGet("enterLevel/{id:int}")]
        public async Task<IActionResult> EnterLevel(int id)
        {
            Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
            if (slot == null) return this.NotFound();

            slot.Plays++;

            await this.database.SaveChangesAsync();

            return this.Ok();
        }
    }
}