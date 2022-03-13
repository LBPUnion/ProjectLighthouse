#nullable enable
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages.Debug;

public class FilterTestPage : BaseLayout
{
    public FilterTestPage(Database database) : base(database)
    {}

    public string? FilteredText = null;
    public string? Text = null;

    public async Task<IActionResult> OnGet(string? text = null)
    {
        #if !DEBUG
        return this.NotFound();
        #endif

        if (text != null) this.FilteredText = CensorHelper.ScanMessage(text);
        this.Text = text;

        return this.Page();
    }
}