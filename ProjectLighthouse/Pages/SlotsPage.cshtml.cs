#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages;

public class SlotsPage : BaseLayout
{

    public int PageAmount;

    public int PageNumber;

    public int SlotCount;

    public List<Slot> Slots;

    public string SearchValue;

    public SlotsPage([NotNull] Database database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int pageNumber, [FromQuery] string? name)
    {
        if (string.IsNullOrWhiteSpace(name)) name = "";

        this.SlotCount = await this.Database.Slots.CountAsync(p => p.Name.Contains(name));

        this.SearchValue = name;
        this.PageNumber = pageNumber;
        this.PageAmount = Math.Max(1, (int) Math.Ceiling((double) this.SlotCount / ServerStatics.PageSize));

        if (this.PageNumber < 0 || this.PageNumber >= this.PageAmount) return this.Redirect($"/slots/{Math.Clamp(this.PageNumber, 0, this.PageAmount - 1)}");

        this.Slots = await this.Database.Slots.Where
                (p => p.Name.Contains(name))
            .OrderByDescending(p => p.FirstUploaded)
            .Skip(pageNumber * ServerStatics.PageSize)
            .Take(ServerStatics.PageSize)
            .ToListAsync();

        return this.Page();
    }
}