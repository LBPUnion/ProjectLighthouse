using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Frames;

public class SlotsFrame : PaginatedFrame
{
    public List<SlotEntity> Slots = new();

    public string Type { get; set; } = "";
    public int Id { get; set; }
    public string Color { get; set; } = "";
    public string SlotsPresentText { get; set; } = "";
    public string SlotsEmptyText { get; set; } = "";

    public SlotsFrame(DatabaseContext database) : base(database)
    {
        this.ItemsPerPage = 10;
    }

    public async Task<IActionResult> OnGet([FromQuery] int page, string route, int id)
    {
        this.Type = route;
        this.Id = id;
        this.CurrentPage = page;
        IQueryable<SlotEntity> slotsQuery = this.Database.Slots.AsQueryable();

        switch (route)
        {
            case "by":
                slotsQuery = this.Database.Slots.Include(p => p.Creator)
                    .Where(p => p.CreatorId == id)
                    .OrderByDescending(s => s.LastUpdated);
                this.Color = "green";
                this.SlotsEmptyText = "This user hasn't published any levels";
                this.SlotsPresentText = "This user has published {0} level{1}";
                break;
            case "hearted":
                slotsQuery = this.Database.HeartedLevels.Where(h => h.UserId == id).
                    OrderByDescending(h => h.HeartedLevelId)
                    .Include(h => h.Slot)
                    .Select(h => h.Slot);
                this.Color = "pink";
                this.SlotsEmptyText = "You haven't hearted any levels";
                this.SlotsPresentText = "You have hearted {0} level{1}";
                break;
            case "queued":
                slotsQuery = this.Database.QueuedLevels.Where(q => q.UserId == id)
                    .OrderByDescending(q => q.QueuedLevelId)
                    .Include(q => q.Slot)
                    .Select(q => q.Slot);
                this.Color = "yellow";
                this.SlotsEmptyText = "You haven't queued any levels";
                this.SlotsPresentText = "There are {0} level{1} in your queue";
                break;
        }

        this.TotalItems = await slotsQuery.CountAsync();

        this.ClampPage();

        if (this.TotalItems != 0)
        {
            this.SlotsPresentText = string.Format(this.SlotsPresentText, this.TotalItems, this.TotalItems == 1 ? "" : "s");
        }

        this.Slots = await slotsQuery
            .ApplyPagination(this.PageData)
            .ToListAsync();

        return this.Page();
    }
}