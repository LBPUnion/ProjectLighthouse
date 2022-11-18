using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Slots;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML")]
[Produces("text/plain")]
public class LevelTagsController : ControllerBase
{
    private readonly Database database;

    public LevelTagsController(Database database)
    {
        this.database = database;
    }

    [HttpGet("tags")]
    public IActionResult Get()
    {
        string[] tags = Enum.GetNames(typeof(LevelTags));

        int i = 0;
        foreach (string tag in tags)
        {
            tags[i] = $"TAG_{tag.Replace("_", "-")}";
            i++;
        }

        return this.Ok(string.Join(",", tags));
    }

    [HttpPost("tag/{slotType}/{id:int}")]
    public async Task<IActionResult> PostTag([FromForm(Name = "t")] string tagName, [FromRoute] string slotType, [FromRoute] int id)
    {
        GameToken token = this.GetToken();

        Slot? slot = await this.database.Slots.Where(s => s.SlotId == id).FirstOrDefaultAsync();
        if (slot == null) return this.BadRequest();

        if (!LabelHelper.IsValidTag(tagName)) return this.BadRequest();

        if (token.UserId == slot.CreatorId) return this.BadRequest();

        if (slot.GameVersion != GameVersion.LittleBigPlanet1) return this.BadRequest();

        if (slotType != "user") return this.BadRequest();

        RatedLevel? rating = await this.database.RatedLevels.FirstOrDefaultAsync(r => r.UserId == token.UserId && r.SlotId == slot.SlotId);
        if (rating == null) return this.BadRequest();

        rating.TagLBP1 = tagName;

        await this.database.SaveChangesAsync();

        return this.Ok();
    }

}