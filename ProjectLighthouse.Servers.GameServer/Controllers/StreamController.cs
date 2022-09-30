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

        IEnumerable<ActivityStream> streamsList = this.database.Stream.Where(s => s.Timestamp < timestamp && s.Timestamp > endTimestamp).ToList();
        string streams = "";
        string news = "";
        string slots = "";
        string users = "";
        foreach (ActivityStream article in streamsList)
        {
            switch (article.PostType)
            {
                case "news_post":
                    News? newsObject = await this.database.News.FirstOrDefaultAsync(n => n.NewsId == article.ReferencedId);
                    if (newsObject != null) news += newsObject.Serialize();
                    break;
                default:
                    Slot? pickedSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == article.ReferencedId);
                    if (pickedSlot == null) break;
                    User? creator = await this.database.Users.Include(u => u.Location).FirstOrDefaultAsync(u => u.UserId == pickedSlot.CreatorId);
                    if (pickedSlot != null && creator != null)
                    {
                        slots += pickedSlot.Serialize();
                        users += creator.Serialize(token.GameVersion);
                    }
                    break;
            }
            streams += article.Serialize();
        }
        return this.Ok(
            LbpSerializer.StringElement("stream",
                LbpSerializer.StringElement("start_timestamp", timestamp) +
                LbpSerializer.StringElement("end_timestamp", endTimestamp) +
                LbpSerializer.StringElement("groups", streams) +
                LbpSerializer.StringElement("news", news) +
                LbpSerializer.StringElement("slots", slots) +
                LbpSerializer.StringElement("users", users)
            )
        );
    }
}