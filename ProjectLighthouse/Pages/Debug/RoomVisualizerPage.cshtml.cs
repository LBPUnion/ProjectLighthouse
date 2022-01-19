#nullable enable
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages.Debug;

public class RoomVisualizerPage : BaseLayout
{
    public RoomVisualizerPage([NotNull] Database database) : base(database)
    {}

    public async Task<IActionResult> OnGet()
    {
        #if !DEBUG
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null || !user.IsAdmin) return this.NotFound();

        return this.Page();
        #else
        return this.Page();
        #endif
    }
}