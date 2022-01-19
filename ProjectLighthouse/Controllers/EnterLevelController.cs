#nullable enable
using System;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers;

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
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slotId);
        if (slot == null) return this.StatusCode(403, "");

        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        GameVersion gameVersion = token.GameVersion;

        IQueryable<VisitedLevel> visited = this.database.VisitedLevels.Where(s => s.SlotId == slotId && s.UserId == user.UserId);
        VisitedLevel? v;
        if (!visited.Any())
        {
            switch (gameVersion)
            {
                case GameVersion.LittleBigPlanet2:
                    slot.PlaysLBP2Unique++;
                    break;
                case GameVersion.LittleBigPlanet3:
                    slot.PlaysLBP3Unique++;
                    break;
                case GameVersion.LittleBigPlanetVita:
                    slot.PlaysLBPVitaUnique++;
                    break;
                default: return this.BadRequest();
            }

            v = new VisitedLevel();
            v.SlotId = slotId;
            v.UserId = user.UserId;
            this.database.VisitedLevels.Add(v);
        }
        else
        {
            v = await visited.FirstOrDefaultAsync();
        }

        if (v == null) return this.NotFound();

        switch (gameVersion)
        {
            case GameVersion.LittleBigPlanet2:
                slot.PlaysLBP2++;
                v.PlaysLBP2++;
                break;
            case GameVersion.LittleBigPlanet3:
                slot.PlaysLBP3++;
                v.PlaysLBP3++;
                break;
            case GameVersion.LittleBigPlanetVita:
                slot.PlaysLBPVita++;
                v.PlaysLBPVita++;
                break;
            case GameVersion.LittleBigPlanetPSP: throw new NotImplementedException();
            case GameVersion.Unknown:
            default:
                return this.BadRequest();
        }

        await this.database.SaveChangesAsync();

        return this.Ok();
    }

    // Only used in LBP1
    [HttpGet("enterLevel/{id:int}")]
    public async Task<IActionResult> EnterLevel(int id)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        IQueryable<VisitedLevel> visited = this.database.VisitedLevels.Where(s => s.SlotId == id && s.UserId == user.UserId);
        VisitedLevel? v;
        if (!visited.Any())
        {
            slot.PlaysLBP1Unique++;

            v = new VisitedLevel();
            v.SlotId = id;
            v.UserId = user.UserId;
            this.database.VisitedLevels.Add(v);
        }
        else
        {
            v = await visited.FirstOrDefaultAsync();
        }

        if (v == null) return this.NotFound();

        slot.PlaysLBP1++;
        v.PlaysLBP1++;

        await this.database.SaveChangesAsync();

        return this.Ok();
    }
}