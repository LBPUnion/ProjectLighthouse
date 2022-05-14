#nullable enable
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LBPUnion.ProjectLighthouse.Website.Pages.Layouts;

public class BaseLayout : PageModel
{

    public readonly Database Database;

    public readonly List<PageNavigationItem> NavigationItems = new()
    {
        new PageNavigationItem("Home", "/", "home"),
        new PageNavigationItem("Users", "/users/0", "user friends"),
        new PageNavigationItem("Photos", "/photos/0", "camera"),
        new PageNavigationItem("Levels", "/slots/0", "certificate"),
    };

    public readonly List<PageNavigationItem> NavigationItemsRight = new();
    public string Description = string.Empty;

    public bool IsMobile;

    public bool ShowTitleInPage = true;

    public string Title = string.Empty;

    private User? user;
    public BaseLayout(Database database)
    {
        this.Database = database;
    }

    public new User? User {
        get {
            if (this.user != null) return this.user;

            return this.user = this.Database.UserFromWebRequest(this.Request);
        }
        set => this.user = value;
    }
}