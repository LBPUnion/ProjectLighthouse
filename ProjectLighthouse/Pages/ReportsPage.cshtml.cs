#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Reports;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages;

public class ReportsPage : BaseLayout
{

    public int PageAmount;

    public int PageNumber;

    public int ReportCount;

    public List<GriefReport> Reports = new();

    public string SearchValue = "";

    public ReportsPage(Database database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int pageNumber, [FromQuery] string? name)
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");
        if (!user.IsAdmin) return this.NotFound();

        if (string.IsNullOrWhiteSpace(name)) name = "";

        this.SearchValue = name.Replace(" ", string.Empty);

        this.ReportCount = await this.Database.Reports.Include(r => r.ReportingPlayer).CountAsync(r => r.ReportingPlayer.Username.Contains(this.SearchValue));

        this.PageNumber = pageNumber;
        this.PageAmount = Math.Max(1, (int)Math.Ceiling((double)this.ReportCount / ServerStatics.PageSize));

        if (this.PageNumber < 0 || this.PageNumber >= this.PageAmount)
            return this.Redirect($"/admin/reports/{Math.Clamp(this.PageNumber, 0, this.PageAmount - 1)}");

        this.Reports = await this.Database.Reports.Include(r => r.ReportingPlayer)
            .Where(r => r.ReportingPlayer.Username.Contains(this.SearchValue))
            .OrderByDescending(r => r.Timestamp)
            .Skip(pageNumber * ServerStatics.PageSize)
            .Take(ServerStatics.PageSize)
            .ToListAsync();

        foreach (GriefReport r in this.Reports)
        {
            r.XmlPlayers = (ReportPlayer[])JsonSerializer.Deserialize(r.Players, typeof(ReportPlayer[]))!;

            r.XmlBounds = new Marqee()
            {
                Rect = (Rectangle)JsonSerializer.Deserialize(r.Bounds, typeof(Rectangle))!,
            };
        }

        return this.Page();
    }
}