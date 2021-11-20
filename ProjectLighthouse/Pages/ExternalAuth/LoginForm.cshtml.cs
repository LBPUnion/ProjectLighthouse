using LBPUnion.ProjectLighthouse.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages.ExternalAuth
{
    public class LoginForm : BaseLayout
    {
        public IActionResult OnGet() => this.Page();
    }
}