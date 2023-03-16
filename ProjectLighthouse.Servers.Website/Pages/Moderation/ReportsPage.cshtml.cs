#nullable enable
using System.Text.Json;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Moderation;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Moderation.Reports;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Moderation;

public class ReportsPage : BaseLayout
{
    public int PageAmount;

    public int PageNumber;

    public int ReportCount;

    public List<GriefReportEntity> Reports = new();

    public string SearchValue = "";

    public ReportsPage(DatabaseContext database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int pageNumber, [FromQuery] string? name)
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");
        if (!user.IsModerator) return this.NotFound();

        if (string.IsNullOrWhiteSpace(name)) name = "";

        this.SearchValue = name.Replace(" ", string.Empty);

        this.ReportCount = await this.Database.Reports.Include(r => r.ReportingPlayer).CountAsync(r => r.ReportingPlayer.Username.Contains(this.SearchValue));

        this.PageNumber = pageNumber;
        this.PageAmount = Math.Max(1, (int)Math.Ceiling((double)this.ReportCount / ServerStatics.PageSize));

        if (this.PageNumber < 0 || this.PageNumber >= this.PageAmount)
            return this.Redirect($"/moderation/reports/{Math.Clamp(this.PageNumber, 0, this.PageAmount - 1)}");

        this.Reports = await this.Database.Reports.Include(r => r.ReportingPlayer)
            .Where(r => r.ReportingPlayer.Username.Contains(this.SearchValue))
            .OrderByDescending(r => r.Timestamp)
            .Skip(pageNumber * ServerStatics.PageSize)
            .Take(ServerStatics.PageSize)
            .ToListAsync();

        foreach (GriefReportEntity r in this.Reports)
        {
            r.XmlPlayers = (ReportPlayer[]?)JsonSerializer.Deserialize(r.Players, typeof(ReportPlayer[])) ?? Array.Empty<ReportPlayer>();

            r.XmlBounds = new Marqee
            {
                Rect = (Rectangle?)JsonSerializer.Deserialize(r.Bounds, typeof(Rectangle)) ?? new Rectangle(),
            };
        }

        return this.Page();
    }
}