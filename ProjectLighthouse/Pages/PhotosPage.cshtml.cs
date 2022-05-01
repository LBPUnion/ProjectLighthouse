#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages;

public class PhotosPage : BaseLayout
{

    public int PageAmount;

    public int PageNumber;

    public int PhotoCount;

    public List<Photo> Photos = new();

    public string? SearchValue;
    public PhotosPage(Database database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int pageNumber, [FromQuery] string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) name = "";

        this.SearchValue = name.Replace(" ", string.Empty);

        this.PhotoCount = await this.Database.Photos.Include
                (p => p.Creator)
            .CountAsync(p => p.Creator!.Username.Contains(this.SearchValue) || p.PhotoSubjectCollection.Contains(this.SearchValue));

        this.PageNumber = pageNumber;
        this.PageAmount = Math.Max(1, (int)Math.Ceiling((double)this.PhotoCount / ServerStatics.PageSize));

        if (this.PageNumber < 0 || this.PageNumber >= this.PageAmount) return this.Redirect($"/photos/{Math.Clamp(this.PageNumber, 0, this.PageAmount - 1)}");

        this.Photos = await this.Database.Photos.Include
                (p => p.Creator)
            .Where(p => p.Creator!.Username.Contains(this.SearchValue) || p.PhotoSubjectCollection.Contains(this.SearchValue))
            .OrderByDescending(p => p.Timestamp)
            .Skip(pageNumber * ServerStatics.PageSize)
            .Take(ServerStatics.PageSize)
            .ToListAsync();

        return this.Page();
    }
}