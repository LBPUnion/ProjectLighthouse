#nullable enable
using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class PhotosPage : BaseLayout
{
    public int PageAmount;

    public int PageNumber;

    public int PhotoCount;

    public List<PhotoEntity> Photos = new();

    public string? SearchValue;
    public PhotosPage(DatabaseContext database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int pageNumber, [FromQuery] string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) name = "";

        IQueryable<PhotoEntity> photos = this.Database.Photos.Include(p => p.Creator)
            .Include(p => p.PhotoSubjects)
            .ThenInclude(ps => ps.User);

        if (name.Contains("by:") || name.Contains("with:"))
        {
            foreach (string part in name.Split(" ", StringSplitOptions.RemoveEmptyEntries))
            {
                if (part.Contains("by:"))
                {
                    photos = photos.Where(p => p.Creator != null && p.Creator.Username.Contains(part.Replace("by:", "")));
                }
                else if (part.Contains("with:"))
                {
                    photos = photos.Where(p => p.PhotoSubjects.Any(ps => ps.User.Username.Contains(part.Replace("with:", ""))));
                }
            }
        }
        else
        {
            photos = photos.Where(p => p.Creator != null && (p.PhotoSubjects.Any(ps => ps.User.Username.Contains(name)) || p.Creator.Username.Contains(name)));
        }

        this.SearchValue = name.Trim();

        this.PhotoCount = await photos.CountAsync();

        this.PageNumber = pageNumber;
        this.PageAmount = Math.Max(1, (int)Math.Ceiling((double)this.PhotoCount / ServerStatics.PageSize));

        if (this.PageNumber < 0 || this.PageNumber >= this.PageAmount) return this.Redirect($"/photos/{Math.Clamp(this.PageNumber, 0, this.PageAmount - 1)}");

        this.Photos = await photos.OrderByDescending(p => p.Timestamp)
            .Skip(pageNumber * ServerStatics.PageSize)
            .Take(ServerStatics.PageSize)
            .ToListAsync();

        return this.Page();
    }
}