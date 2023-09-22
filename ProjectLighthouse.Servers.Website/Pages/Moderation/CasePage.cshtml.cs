using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Moderation;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Moderation;

public class CasePage : BaseLayout
{
    public CasePage(DatabaseContext database) : base(database)
    { }

    public List<ModerationCaseEntity> Cases = new();
    public int CaseCount;
    public int ExpiredCaseCount;
    public int DismissedCaseCount;

    public int PageAmount;
    public int PageNumber;
    public string SearchValue = "";

    public async Task<IActionResult> OnGet([FromRoute] int pageNumber, [FromQuery] string? name)
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.NotFound();
        if (!user.IsModerator) return this.NotFound();

        if (string.IsNullOrWhiteSpace(name)) name = "";

        this.SearchValue = name.Replace(" ", string.Empty);

        this.CaseCount =
            await this.Database.Cases.CountAsync(c =>
                c.AffectedId.ToString().Contains(this.SearchValue));
        this.ExpiredCaseCount =
            await this.Database.Cases.CountAsync(c =>
                c.AffectedId.ToString().Contains(this.SearchValue) && c.DismissedAt == null && c.ExpiresAt < DateTime.UtcNow);
        this.DismissedCaseCount =
            await this.Database.Cases.CountAsync(c =>
                c.AffectedId.ToString().Contains(this.SearchValue)&& c.DismissedAt != null);

        this.PageNumber = pageNumber;
        this.PageAmount = Math.Max(1, (int)Math.Ceiling((double)this.CaseCount / ServerStatics.PageSize));

        if (this.PageNumber < 0 || this.PageNumber >= this.PageAmount)
            return this.Redirect($"/moderation/cases/{Math.Clamp(this.PageNumber, 0, this.PageAmount - 1)}");

        this.Cases = await this.Database.Cases.Include(c => c.Creator)
            .Include(c => c.Dismisser)
            .Where(c => c.AffectedId.ToString().Contains(this.SearchValue))
            .OrderByDescending(c => c.CaseId)
            .Skip(pageNumber * ServerStatics.PageSize)
            .Take(ServerStatics.PageSize)
            .ToListAsync();

        return this.Page();
    }
}