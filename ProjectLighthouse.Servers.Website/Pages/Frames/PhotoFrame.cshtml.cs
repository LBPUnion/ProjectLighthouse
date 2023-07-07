using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Frames;

public class PhotoFrame : BaseFrame
{
    public List<PhotoEntity> Photos { get; set; } = new();

    public PhotoFrame(DatabaseContext database) : base(database)
    { }

    public async Task<IActionResult> OnGet(string type, int id)
    {
        if (type != "user" && type != "slot") return this.BadRequest();

        IQueryable<PhotoEntity> photoQuery = this.Database.Photos.Include(p => p.Slot)
            .Include(p => p.PhotoSubjects)
            .ThenInclude(ps => ps.User)
            .OrderByDescending(p => p.Timestamp);

        switch (type)
        {
            case "user":
                photoQuery = photoQuery.Where(p => p.CreatorId == id);
                break;
            
            case "slot":
                photoQuery = photoQuery.Where(p => p.SlotId == id);
                break;
        }

        this.Photos = await photoQuery.Take(6).ToListAsync();

        return this.Page();
    }
}