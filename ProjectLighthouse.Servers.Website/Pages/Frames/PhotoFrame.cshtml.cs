using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Frames;

public class PhotoFrame : PaginatedFrame
{
    public List<PhotoEntity> Photos { get; set; } = new();

    public PhotoFrame(DatabaseContext database) : base(database)
    {
        this.ItemsPerPage = 10;
    }

    public async Task<IActionResult> OnGet([FromQuery] int page, string type, int id)
    {
        this.CurrentPage = page;
        if (type != "user" && type != "slot") return this.BadRequest();

        IQueryable<PhotoEntity> photoQuery = this.Database.Photos.Include(p => p.Slot)
            .Include(p => p.PhotoSubjects)
            .ThenInclude(ps => ps.User);

        switch (type)
        {
            case "user":
                photoQuery = photoQuery.Where(p => p.CreatorId == id);
                break;
            
            case "slot":
                photoQuery = photoQuery.Where(p => p.SlotId == id);
                break;
        }

        this.TotalItems = await photoQuery.CountAsync();

        this.ClampPage();

        this.Photos = await photoQuery.OrderByDescending(p => p.Timestamp).ApplyPagination(this.PageData).ToListAsync();

        return this.Page();
    }
}