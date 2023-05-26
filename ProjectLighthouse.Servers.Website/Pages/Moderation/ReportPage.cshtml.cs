using System.Text.Json;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Moderation;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Moderation.Reports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Moderation;

public class ReportPage : BaseLayout
{
    public ReportPage(DatabaseContext database) : base(database)
    {}

    public GriefReportEntity Report = null!; // Report is not used if it's null in OnGet
    
    public async Task<IActionResult> OnGet([FromRoute] int reportId)
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");
        if (!user.IsModerator) return this.NotFound();

        GriefReportEntity? report = await this.Database.Reports
            .Include(r => r.ReportingPlayer)
            .FirstOrDefaultAsync(r => r.ReportId == reportId);
        if (report == null) return this.NotFound();

        report.XmlPlayers = (ReportPlayer[]?)JsonSerializer.Deserialize(report.Players,
            typeof(ReportPlayer[])) ?? Array.Empty<ReportPlayer>();

        report.XmlBounds = new Marqee
        {
            Rect = (Rectangle?)JsonSerializer.Deserialize(report.Bounds,
                typeof(Rectangle)) ?? new Rectangle(),
        };

        this.Report = report;

        return this.Page();
    }
}