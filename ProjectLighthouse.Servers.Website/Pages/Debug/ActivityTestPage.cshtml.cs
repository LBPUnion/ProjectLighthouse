using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Activity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Debug;

public class ActivityTestPage : BaseLayout
{
    public ActivityTestPage(DatabaseContext database) : base(database)
    { }

    public List<OuterActivityGroup> ActivityGroups = [];

    public bool GroupByActor { get; set; }

    public async Task<IActionResult> OnGet(bool groupByActor = false)
    {
        Console.WriteLine(groupByActor);
        List<OuterActivityGroup>? events = (await this.Database.Activities.ToActivityDto(true).ToActivityGroups(groupByActor).ToListAsync())
            .ToOuterActivityGroups(groupByActor);

        if (events == null) return this.Page();

        this.GroupByActor = groupByActor;

        this.ActivityGroups = events;
        return this.Page();
    }

    
}