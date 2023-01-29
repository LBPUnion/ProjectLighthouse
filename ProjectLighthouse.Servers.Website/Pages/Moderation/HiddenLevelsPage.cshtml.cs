using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Entities.Level;
using LBPUnion.ProjectLighthouse.Entities.Token;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Moderation;

public class HiddenLevelsPage : BaseLayout
{
    public HiddenLevelsPage(Database database) : base(database)
    {}

    public int PageAmount;

    public int PageNumber;

    public int SlotCount;

    public List<Slot> Slots = new();

    public async Task<IActionResult> OnGet([FromRoute] int pageNumber, [FromQuery] string? name)
    {
        WebToken? token = this.Database.WebTokenFromRequest(this.Request);
        if (token == null) return this.Redirect("/login");

        this.Slots = await this.Database.Slots
            .Where(s => s.Hidden)
            .Skip(pageNumber * ServerStatics.PageSize)
            .Take(ServerStatics.PageSize)
            .ToListAsync();
        
        this.SlotCount = await this.Database.Slots.CountAsync(s => s.Hidden);

        this.PageAmount = Math.Max(1, (int)Math.Ceiling((double)this.SlotCount / ServerStatics.PageSize));

        return this.Page();
    }
}