using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Types.Entities.Level;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Users;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Frames;

public class PhotoFrame : PaginatedFrame
{
    public List<PhotoEntity> Photos { get; set; } = new();

    public bool CanViewPhotos { get; set; }
    public string Type { get; set; } = "";

    public PhotoFrame(DatabaseContext database) : base(database)
    {
        this.ItemsPerPage = 10;
    }

    public async Task<IActionResult> OnGet([FromQuery] int page, string type, int id)
    {
        this.CurrentPage = page;
        this.Type = type;
        if (type != "user" && type != "slot") return this.BadRequest();

        IQueryable<PhotoEntity> photoQuery = this.Database.Photos.Include(p => p.Slot)
            .Include(p => p.PhotoSubjects)
            .ThenInclude(ps => ps.User);

        switch (type)
        {
            case "user":
                UserEntity? user = await this.Database.Users.FindAsync(id);
                if (user == null) return this.NotFound();
                this.CanViewPhotos = user.ProfileVisibility.CanAccess(this.User != null,
                    this.User == user || this.User != null && this.User.IsModerator);
                photoQuery = photoQuery.Where(p => p.CreatorId == id);
                break;
            case "slot":
                SlotEntity? slot = await this.Database.Slots.Include(s => s.Creator)
                    .Where(s => s.SlotId == id)
                    .FirstOrDefaultAsync();
                if (slot == null || slot.Creator == null) return this.NotFound();
                this.CanViewPhotos = slot.Creator.LevelVisibility.CanAccess(this.User != null,
                    this.User == slot.Creator || this.User != null && this.User.IsModerator);
                photoQuery = photoQuery.Where(p => p.SlotId == id);
                break;
        }

        if (!this.CanViewPhotos)
        {
            this.ClampPage();
            return this.Page();
        }

        this.TotalItems = await photoQuery.CountAsync();

        this.ClampPage();

        this.Photos = await photoQuery.OrderByDescending(p => p.Timestamp).ApplyPagination(this.PageData).ToListAsync();

        return this.Page();
    }
}