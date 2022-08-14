using System.Text.Json;
using LBPUnion.ProjectLighthouse.Administration.Reports;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Moderation;

public class ReportPage : BaseLayout
{
    public ReportPage(Database database) : base(database)
    {}

    public GriefReport Report = null!; // Report is not used if it's null in OnGet
    
    public async Task<IActionResult> OnGet([FromRoute] int reportId)
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");
        if (!user.IsAdmin) return this.NotFound();

        GriefReport? report = await this.Database.Reports
            .Include(r => r.ReportingPlayer)
            .FirstOrDefaultAsync(r => r.ReportId == reportId);
        if (report == null) return this.NotFound();

        report.XmlPlayers = (ReportPlayer[])JsonSerializer.Deserialize(report.Players,
            typeof(ReportPlayer[]))!;

        report.XmlBounds = new Marqee
        {
            Rect = (Rectangle)JsonSerializer.Deserialize(report.Bounds,
                typeof(Rectangle))!,
        };

        this.Report = report;

        return this.Page();
    }
}