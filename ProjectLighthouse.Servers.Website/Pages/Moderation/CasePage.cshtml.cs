using LBPUnion.ProjectLighthouse.Administration;
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Moderation;

public class CasePage : BaseLayout
{
    public CasePage(Database database) : base(database)
    {}

    public List<ModerationCase> Cases = new();
    public int CaseCount;
    public int DismissedCaseCount;

    public int PageAmount;
    public int PageNumber;
    public string SearchValue = "";

    public async Task<IActionResult> OnGet([FromRoute] int pageNumber, [FromQuery] string? name)
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.NotFound();
        if (!user.IsModerator) return this.NotFound();

        if (string.IsNullOrWhiteSpace(name)) name = "";

        this.SearchValue = name.Replace(" ", string.Empty);

        this.Cases = await this.Database.Cases
            .Include(c => c.Creator)
            .Include(c => c.Dismisser)
            .OrderByDescending(c => c.CaseId)
            .ToListAsync();
        
        this.CaseCount = await this.Database.Cases.CountAsync(c => c.Reason.Contains(this.SearchValue));
        this.DismissedCaseCount = await this.Database.Cases.CountAsync(c => c.Reason.Contains(this.SearchValue) && c.DismissedAt != null);

        this.PageNumber = pageNumber;
        this.PageAmount = Math.Max(1, (int)Math.Ceiling((double)this.CaseCount / ServerStatics.PageSize));

        return this.Page();
    }
}