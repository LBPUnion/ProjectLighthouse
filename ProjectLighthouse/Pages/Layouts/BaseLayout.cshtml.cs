#nullable enable
using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LBPUnion.ProjectLighthouse.Pages.Layouts
{
    public class BaseLayout : PageModel
    {
        public readonly Database Database;

        public new User? User { get; set; }

        public BaseLayout(Database database)
        {
            this.Database = database;
        }

        public readonly List<PageNavigationItem> NavigationItems = new()
        {
            new PageNavigationItem("Home", "/"),
        };

    }
}