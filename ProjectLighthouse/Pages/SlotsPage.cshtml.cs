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
    public SlotsPage([NotNull] Database database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromRoute] int pageNumber)
    {
        this.SlotCount = await StatisticsHelper.SlotCount();

        this.PageNumber = pageNumber;
        this.PageAmount = (int)Math.Ceiling((double)this.SlotCount / ServerStatics.PageSize);

        if (this.PageNumber < 0 || this.PageNumber >= this.PageAmount) return this.Redirect($"/slots/{Math.Clamp(this.PageNumber, 0, this.PageAmount - 1)}");

        this.Slots = await this.Database.Slots.Include
                (p => p.Creator)
            .OrderByDescending(p => p.FirstUploaded)
            .Skip(pageNumber * ServerStatics.PageSize)
            .Take(ServerStatics.PageSize)
            .ToListAsync();

        return this.Page();
    }
}