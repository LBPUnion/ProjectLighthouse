#nullable enable
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Types.Entities.Moderation;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers.Admin;

[ApiController]
[Route("/moderation/report/{id:int}")]
public class AdminReportController : ControllerBase
{
    private readonly DatabaseContext database;

    public AdminReportController(DatabaseContext database)
    {
        this.database = database;
    }

    [HttpGet("remove")]
    public async Task<IActionResult> DeleteReport([FromRoute] int id)
    {
        UserEntity? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.StatusCode(403);

        GriefReportEntity? report = await this.database.Reports.FirstOrDefaultAsync(r => r.ReportId == id);
        if (report == null) return this.NotFound();

        List<string> hashes = new()
        {
            report.JpegHash,
            report.GriefStateHash,
        };
        if (report.LevelType != "user")
            hashes.Add(report.InitialStateHash);
        foreach (string hash in hashes)
        {
            FileHelper.DeleteResource(hash);
        }
        this.database.Reports.Remove(report);

        await this.database.SaveChangesAsync();

        return this.Redirect("~/moderation/reports/0");
    }

    [HttpGet("dismiss")]
    public async Task<IActionResult> DismissReport([FromRoute] int id)
    {
        UserEntity? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsModerator) return this.StatusCode(403);

        GriefReportEntity? report = await this.database.Reports.FirstOrDefaultAsync(r => r.ReportId == id);
        if (report == null) return this.NotFound();

        FileHelper.DeleteResource(report.JpegHash);

        this.database.Reports.Remove(report);

        await this.database.SaveChangesAsync();

        return this.Redirect("~/moderation/reports/0");
    }
}