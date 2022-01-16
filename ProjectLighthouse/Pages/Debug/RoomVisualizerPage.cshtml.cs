using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages.Debug;

public class RoomVisualizerPage : BaseLayout
{
    public RoomVisualizerPage([NotNull] Database database) : base(database)
    {}

    public async Task<IActionResult> OnGet()
    {
        #if !DEBUG
        return this.NotFound();
        #else
        return this.Page();
        #endif
    }
}