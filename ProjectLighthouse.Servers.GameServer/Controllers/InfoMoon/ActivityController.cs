using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.InfoMoon;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class ActivityController : ControllerBase
{
    private readonly Database database;
    public ActivityController(Database database)
    {
        this.database = database;
    }

    [HttpPost("stream")]
    [HttpGet("stream")]
    public async Task<IActionResult> ActivityStream([FromQuery] long timestamp, [FromQuery] long endTimestamp)
    {
        // endTimestamp will report as 0 occasionally, this is automagically handled as 0
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");
        GameVersion gameVersion = token.GameVersion;

        // TODO: Filter. Don't show events from everyone.
        IEnumerable<Activity> streamsList = this.database.Activity
            .Where(a => a.Timestamp < timestamp && a.Timestamp > endTimestamp)
            .OrderByDescending(a => a.Timestamp)
            .ToList();
        string streams = "";
        string news = "";
        string slots = "";
        string users = "";
        foreach (Activity article in streamsList)
        {
            streams += await article.SerializeAsync();
            switch((ActivityCategory)article.Category)
            {
                case ActivityCategory.TeamPick:
                case ActivityCategory.Level:
                    Slot? slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == article.DestinationId);
                    slots += slot?.Serialize();
                    foreach (int id in article.Actors)
                    {
                        User? user = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == id);
                        users += user?.Serialize();
                    }
                    break;
                case ActivityCategory.News:
                    news += "";
                    break;
                case ActivityCategory.User:
                    foreach (int id in article.Actors)
                    {
                        User? user = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == id);
                        users += user?.Serialize();
                    }
                    break;
            }
        }
        return this.Ok(
            LbpSerializer.StringElement("stream",
                LbpSerializer.StringElement("start_timestamp", timestamp) +
                LbpSerializer.StringElement("end_timestamp", endTimestamp) +
                LbpSerializer.StringElement("groups", streams) +
                ((news != "") ? LbpSerializer.StringElement("news", news) : "") +
                ((slots != "") ? LbpSerializer.StringElement("slots", slots) : "") +
                ((users != "") ? LbpSerializer.StringElement("users", users) : "")
            )
        );
    }
    // Database.cs logic will be moved here in the future
}