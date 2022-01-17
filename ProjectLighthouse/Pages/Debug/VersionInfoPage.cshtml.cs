using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages.Debug;

public class VersionInfoPage : BaseLayout
{
    public VersionInfoPage([NotNull] Database database) : base(database)
    {}
    public IActionResult OnGet() => this.Page();
}