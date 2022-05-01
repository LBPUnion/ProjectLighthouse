#nullable enable
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages.Debug;

public class RoomVisualizerPage : BaseLayout
{
    public RoomVisualizerPage(Database database) : base(database)
    {}

    public IActionResult OnGet()
    {
        #if !DEBUG
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.NotFound();
        #endif

        return this.Page();
    }
}