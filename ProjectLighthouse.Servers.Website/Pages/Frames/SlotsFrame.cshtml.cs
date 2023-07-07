using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Frames;

public class SlotsFrame : BaseFrame
{
    public List<SlotEntity> Slots = new();

    public SlotsFrame(DatabaseContext database) : base(database)
    { }

    public async Task<IActionResult> OnGet(string route, int id)
    {
        IQueryable<SlotEntity> slotsQuery = this.Database.Slots.AsQueryable();

        switch (route)
        {
            case "by":
                slotsQuery = this.Database.Slots.Include(p => p.Creator)
                    .OrderByDescending(s => s.LastUpdated)
                    .Where(p => p.CreatorId == id);
                break;
        }

        this.Slots = await slotsQuery.Take(10).ToListAsync();
        return this.Page();
    }
}