#nullable enable
#if DEBUG
using LBPUnion.ProjectLighthouse.Helpers;
#endif
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Types.Filter;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Debug;

public class FilterTestPage : BaseLayout
{
    public FilterTestPage(DatabaseContext database) : base(database)
    {}

    public string? FilteredText;
    public string? Text;

    public IActionResult OnGet(string? text = null)
    {
        #if DEBUG
        if (text != null) this.FilteredText = CensorHelper.FilterMessage(text, FilterLocation.Test);
        this.Text = text;

        return this.Page();
        #else
        return this.NotFound();
        #endif
    }
}