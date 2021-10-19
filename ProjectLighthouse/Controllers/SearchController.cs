using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectLighthouse.Serialization;
using ProjectLighthouse.Types;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/xml")]
    public class SearchController : ControllerBase {
        private readonly Database database;
        public SearchController(Database database) {
            this.database = database;
        }

        [HttpGet("slots/search")]
        public async Task<IActionResult> SearchSlots([FromQuery] string query) {
            query = query.ToLower();

            string[] keywords = query.Split(" ");

            IQueryable<Slot> dbQuery = this.database.Slots
                .Include(s => s.Creator)
                .Include(s => s.Location)
                .Where(s => s.SlotId >= 0); // dumb query to conv into IQueryable
            
            // ReSharper disable once LoopCanBeConvertedToQuery
            foreach(string keyword in keywords) {
                dbQuery = dbQuery.Where(s => 
                    s.Name.ToLower().Contains(keyword) || 
                    s.Description.ToLower().Contains(keyword)
                );
            }

            List<Slot> slots = await dbQuery.ToListAsync();
            string response = slots.Aggregate("", (current, slot) => current + slot.Serialize());

            return this.Ok(LbpSerializer.StringElement("slots", response));
        }
    }
}