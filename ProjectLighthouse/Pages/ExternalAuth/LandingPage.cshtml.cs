using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages.ExternalAuth
{
    public class LandingPage : BaseLayout
    {
        [UsedImplicitly]
        public IActionResult OnGet() => this.Page();
    }
}