using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers
{
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/plain")]
    public class StatisticsController : ControllerBase
    {
        private readonly Database database;
        public StatisticsController(Database database)
        {
            this.database = database;
        }

        [HttpGet("playersInPodCount")]
        [HttpGet("totalPlayerCount")]
        public async Task<IActionResult> TotalPlayerCount()
        {
            int recentMatches = await this.database.LastMatches.Where(l => TimestampHelper.Timestamp - l.Timestamp < 60).CountAsync();

            return this.Ok(recentMatches.ToString());
        }

        [HttpGet("planetStats")]
        public async Task<IActionResult> PlanetStats()
        {
            int totalSlotCount = await this.database.Slots.CountAsync();
            const int mmPicksCount = 0;

            return this.Ok
            (
                LbpSerializer.StringElement
                    ("planetStats", LbpSerializer.StringElement("totalSlotCount", totalSlotCount) + LbpSerializer.StringElement("mmPicksCount", mmPicksCount))
            );
        }
    }
}