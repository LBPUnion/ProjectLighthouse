using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Debug;

public class VersionInfoPage : BaseLayout
{
    public VersionInfoPage([NotNull] Database database) : base(database)
    {}
    public IActionResult OnGet() => this.Page();
}