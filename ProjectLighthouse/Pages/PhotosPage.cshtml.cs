#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
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

    public List<Photo> Photos;

    public string SearchValue;
    public PhotosPage([NotNull] Database database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int pageNumber, [FromQuery] string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) name = "";

        this.PhotoCount = await this.Database.Photos.CountAsync(p => p.Creator.Username.Contains(name) || p.PhotoSubjectCollection.Contains(name));

        this.SearchValue = name;
        this.PageNumber = pageNumber;
        this.PageAmount = Math.Max(1, (int)Math.Ceiling((double)this.PhotoCount / ServerStatics.PageSize));

        if (this.PageNumber < 0 || this.PageNumber >= this.PageAmount) return this.Redirect($"/photos/{Math.Clamp(this.PageNumber, 0, this.PageAmount - 1)}");

        this.Photos = await this.Database.Photos.Include(p => p.Creator)
            .Where(p => p.Creator.Username.Contains(name) || p.PhotoSubjectCollection.Contains(name))
            .OrderByDescending(p => p.Timestamp)
            .Skip(pageNumber * ServerStatics.PageSize)
            .Take(ServerStatics.PageSize)
            .ToListAsync();

        return this.Page();
    }
}