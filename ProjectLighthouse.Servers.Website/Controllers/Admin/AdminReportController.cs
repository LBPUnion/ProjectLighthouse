#nullable enable
using LBPUnion.ProjectLighthouse.Administration.Reports;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers.Admin;

[ApiController]
[Route("/modPanel/report/{id:int}")]
public class AdminReportController : ControllerBase
{
    private readonly Database database;

    public AdminReportController(Database database)
    {
        this.database = database;
    }

    [HttpGet("remove")]
    public async Task<IActionResult> DeleteReport([FromRoute] int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.StatusCode(403, "");

        GriefReport? report = await this.database.Reports.FirstOrDefaultAsync(r => r.ReportId == id);
        if (report == null) return this.NotFound();

        List<string> hashes = new()
        {
            report.JpegHash,
            report.GriefStateHash,
            report.InitialStateHash,
        };
        foreach (string hash in hashes)
        {
            if (System.IO.File.Exists(Path.Combine("png", $"{hash}.png")))
            {
                System.IO.File.Delete(Path.Combine("png", $"{hash}.png"));
            }
            if (System.IO.File.Exists(Path.Combine("r", hash)))
            {
                System.IO.File.Delete(Path.Combine("r", hash));
            }
        }
        this.database.Reports.Remove(report);

        await this.database.SaveChangesAsync();

        return this.Redirect("~/admin/reports/0");
    }

    [HttpGet("dismiss")]
    public async Task<IActionResult> DismissReport([FromRoute] int id)
    {
        User? user = this.database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.StatusCode(403, "");

        GriefReport? report = await this.database.Reports.FirstOrDefaultAsync(r => r.ReportId == id);
        if (report == null) return this.NotFound();

        if (System.IO.File.Exists(Path.Combine("png", $"{report.JpegHash}.png")))
        {
            System.IO.File.Delete(Path.Combine("png", $"{report.JpegHash}.png"));
        }
        if (System.IO.File.Exists(Path.Combine("r", report.JpegHash)))
        {
            System.IO.File.Delete(Path.Combine("r", report.JpegHash));
        }

        this.database.Reports.Remove(report);

        await this.database.SaveChangesAsync();

        return this.Redirect("~/modPanel/reports/0");
    }
}