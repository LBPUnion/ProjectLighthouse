#nullable enable
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers
{
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
//    [Produces("text/plain")]
    public class EnterLevelController : ControllerBase
    {
        private readonly Database database;

        public EnterLevelController(Database database)
        {
            this.database = database;
        }


        [HttpPost("play/user/{slotId}")]
        public async Task<IActionResult> PlayLevel(int slotId)
        {
            User? user = await this.database.UserFromRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slotId);
            if (slot == null) return this.StatusCode(403, "");

            Token? token = await this.database.TokenFromRequest(this.Request);
            if (token == null) return this.StatusCode(403, "");

            GameVersion gameVersion = token.GameVersion;

            IQueryable<VisitedLevel> visited = this.database.VisitedLevels.Where(s => s.SlotId == slotId && s.UserId == user.UserId && s.GameVersion == gameVersion);
            if (!visited.Any())
            {
                switch (gameVersion)
                {
                    case GameVersion.LittleBigPlanet2:
                        slot.PlaysLBP2Unique++;
                        break;
                    case GameVersion.LittleBigPlanetVita:
                        slot.PlaysLBP2Unique++;
                        break;
                    case GameVersion.LittleBigPlanet3:
                        slot.PlaysLBP3Unique++;
                        break;
                    default:
                        return this.BadRequest();
                }

                VisitedLevel v = new();
                v.SlotId = slotId;
                v.UserId = user.UserId;
                v.GameVersion = gameVersion;
                this.database.VisitedLevels.Add(v);
                await this.database.SaveChangesAsync();

            }

            switch (gameVersion)
            {
                case GameVersion.LittleBigPlanet2:
                    slot.PlaysLBP2++;
                    break;
                case GameVersion.LittleBigPlanet3:
                    slot.PlaysLBP3++;
                    break;
                default:
                    return this.BadRequest();
            }

            return this.Ok();
        }

        // Only used in LBP1
        [HttpGet("enterLevel/{id:int}")]
        public async Task<IActionResult> EnterLevel(int id)
        {
            User? user = await this.database.UserFromRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
            if (slot == null) return this.NotFound();

            IQueryable<VisitedLevel> visited = this.database.VisitedLevels.Where(s => s.SlotId == id && s.UserId == user.UserId && s.GameVersion == GameVersion.LittleBigPlanet1);
            if (!visited.Any())
            {
                slot.PlaysLBP1Unique++;

                VisitedLevel v = new();
                v.SlotId = id;
                v.UserId = user.UserId;
                v.GameVersion = GameVersion.LittleBigPlanet1;
                this.database.VisitedLevels.Add(v);

            }

            slot.PlaysLBP1++;

            await this.database.SaveChangesAsync();

            return this.Ok();
        }
    }
}
