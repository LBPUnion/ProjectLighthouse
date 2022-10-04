using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Levels;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

[ApiController]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class StreamController : ControllerBase
{
    private readonly Database database;
    public StreamController(Database database)
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

        IEnumerable<ActivityStream> streamsList = this.database.Stream
            .Include(s => s.Actor)
            .Where(s => s.Timestamp < timestamp && s.Timestamp > endTimestamp)
            .OrderByDescending(s => s.Timestamp)
            .ToList();
        string streams = "";
        string news = "";
        string slots = "";
        string users = "";
        foreach (ActivityStream article in streamsList)
        {
            string[] eventTypes = article.EventTypes;
            switch (eventTypes[0])
            {
                case "news_post":
                    News? newsObject = await this.database.News.FirstOrDefaultAsync(n => n.NewsId == article.TargetId);
                    if (newsObject != null) news += newsObject.Serialize();
                    break;
                default:
                    Slot? pickedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == article.TargetId);
                    if (pickedSlot == null) break;
                    User? creator = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == pickedSlot.CreatorId);
                    if (pickedSlot != null && creator != null)
                    {
                        slots += pickedSlot.Serialize(GameVersion.LittleBigPlanet3, fullSerialization: true);
                        users += creator.Serialize(token.GameVersion);
                    }
                    break;
            }
            streams += await article.Serialize();
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
}