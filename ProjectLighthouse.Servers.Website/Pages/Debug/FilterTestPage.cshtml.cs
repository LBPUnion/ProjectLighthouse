#nullable enable
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Debug;

public class FilterTestPage : BaseLayout
{
    public FilterTestPage(Database database) : base(database)
    {}

    public string? FilteredText;
    public string? Text;
    #if DEBUG
    public IActionResult OnGet(string? text = null)
    {
        if (text != null) this.FilteredText = CensorHelper.ScanMessage(text);
        this.Text = text;

        return this.Page();
    }
    #endif
}