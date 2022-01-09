#nullable enable
using System.Collections.Generic;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LBPUnion.ProjectLighthouse.Pages.Layouts
{
    public class BaseLayout : PageModel
    {
        public BaseLayout(Database database)
        {
            this.Database = database;
        }

        public bool IsMobile;

        public readonly Database Database;

        public readonly List<PageNavigationItem> NavigationItems = new()
        {
            new PageNavigationItem("Home", "/", "home"),
            new PageNavigationItem("Photos", "/photos/0", "camera"),
            new PageNavigationItem("Levels", "/slots/0", "certificate"),
        };

        public readonly List<PageNavigationItem> NavigationItemsRight = new();

        public bool ShowTitleInPage = true;

        public string Title = string.Empty;
        public string Description = string.Empty;

        private User? user;

        public new User? User {
            get {
                if (this.user != null) return this.user;

                return this.user = this.Database.UserFromWebRequest(this.Request);
            }
            set => this.user = value;
        }
    }
}