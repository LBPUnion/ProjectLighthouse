using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Entities.Website;
using LBPUnion.ProjectLighthouse.Types.Serialization.News;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

[ApiController]
[Authorize]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class NewsController : ControllerBase
{
    private readonly DatabaseContext database;

    public NewsController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpGet("news")]
    public async Task<IActionResult> GetNews()
    {
        List<WebsiteAnnouncementEntity> websiteAnnouncements =
            await this.database.WebsiteAnnouncements.OrderByDescending(a => a.AnnouncementId).ToListAsync();

        return this.Ok(GameNews.CreateFromEntity(websiteAnnouncements));
    }
}