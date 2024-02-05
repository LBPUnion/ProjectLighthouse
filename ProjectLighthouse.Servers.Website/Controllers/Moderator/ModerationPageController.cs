using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Levels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers.Moderator;

[ApiController]
[Route("moderation")]
public class ModerationPageController : ControllerBase
{
    private readonly DatabaseContext database;

    public ModerationPageController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpPost("findStoryLevel")]
    public async Task<IActionResult> FindStorySlot([FromForm] int? placeholderSlotId)
    {
        UserEntity? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");

        if (!user.IsModerator) return this.Redirect("~/");

        int slotId = await this.database.Slots.Where(s => s.Type == SlotType.Developer)
            .Where(s => s.InternalSlotId == placeholderSlotId)
            .Select(s => s.SlotId)
            .FirstOrDefaultAsync();

        return this.Redirect(slotId == 0 ? "~/moderation" : $"~/slot/{slotId}");
    }
}