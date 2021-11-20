using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LBPUnion.ProjectLighthouse.Pages.Layouts
{
    public class BaseLayout : PageModel
    {
        public readonly List<PageNavigationItem> NavigationItems = new()
        {
            new PageNavigationItem("Home", "/"),
            new PageNavigationItem("Register", "/register"),
            new PageNavigationItem("Login", "/login"),
        };
    }
}