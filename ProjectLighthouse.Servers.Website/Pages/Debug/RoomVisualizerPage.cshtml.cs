#nullable enable

using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
#if !DEBUG
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
#endif

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Debug;

public class RoomVisualizerPage : BaseLayout
{
    public RoomVisualizerPage(DatabaseContext database) : base(database)
    {}

    public IActionResult OnGet()
    {
        #if !DEBUG
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.NotFound();
        #endif

        return this.Page();
    }
}