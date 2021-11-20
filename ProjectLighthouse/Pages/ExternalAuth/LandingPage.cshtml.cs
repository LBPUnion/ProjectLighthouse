using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LBPUnion.ProjectLighthouse.Controllers.ExternalAuth
{
    public class LandingPage : PageModel
    {
        public IActionResult OnGet() => this.Page();
    }
}