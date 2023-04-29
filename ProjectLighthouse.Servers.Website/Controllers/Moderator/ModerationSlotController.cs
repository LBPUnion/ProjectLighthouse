#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers.Moderator;

[ApiController]
[Route("moderation/slot/{id:int}")]
public class ModerationSlotController : ControllerBase
{
    private readonly DatabaseContext database;

    public ModerationSlotController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpGet("teamPick")]
    public async Task<IActionResult> TeamPick([FromRoute] int id)
    {
        UserEntity? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsModerator) return this.StatusCode(403);

        SlotEntity? slot = await this.database.Slots.Include(s => s.Creator).FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        slot.TeamPick = true;

        // Send webhook with slot.Name and slot.Creator.Username
        await WebhookHelper.SendWebhook("New Team Pick!", $"The level [**{slot.Name}**]({ServerConfiguration.Instance.ExternalUrl}/slot/{slot.SlotId}) by **{slot.Creator?.Username}** has been team picked");

        await this.database.SaveChangesAsync();

        return this.Redirect("~/slot/" + id);
    }

    [HttpGet("removeTeamPick")]
    public async Task<IActionResult> RemoveTeamPick([FromRoute] int id)
    {
        UserEntity? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsModerator) return this.StatusCode(403);

        SlotEntity? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.NotFound();

        slot.TeamPick = false;

        await this.database.SaveChangesAsync();

        return this.Redirect("~/slot/" + id);
    }

    [HttpGet("delete")]
    public async Task<IActionResult> DeleteLevel([FromRoute] int id)
    {
        UserEntity? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsModerator) return this.StatusCode(403);

        SlotEntity? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.Ok();

        await this.database.RemoveSlot(slot);

        return this.Redirect("~/slots/0");
    }

    [HttpGet("flag")]
    public async Task<IActionResult> FlagLevel([FromRoute] int id)
    {
        UserEntity? user = this.database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect($"~/slot/{id}");

        SlotEntity? slot = await this.database.Slots.Include(s => s.Creator).FirstOrDefaultAsync(s => s.SlotId == id);
        if (slot == null) return this.BadRequest();
        if (slot.CreatorId == user.UserId) return this.Redirect($"~/slot/{slot.SlotId}");

        string externalUrl = ServerConfiguration.Instance.ExternalUrl;

        await WebhookHelper.SendWebhook(title: "New duplicate level flag",
            description: @$"Level [**{slot.Name}**]({externalUrl}/slot/{slot.SlotId}) has been flagged as a duplicate level.
                            
                            > **Reporter:** [{user.Username}]({externalUrl}/user/{user.UserId})
                            > **Offender:** [{slot.Creator!.Username}]({externalUrl}/user/{slot.CreatorId})
                            > **Level Hash:** {slot.RootLevel}",
            dest: WebhookHelper.WebhookDestination.Moderation);

        return this.Redirect($"~/slot/{slot.SlotId}");
    }
}
