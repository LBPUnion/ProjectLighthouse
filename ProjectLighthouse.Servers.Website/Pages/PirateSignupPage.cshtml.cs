using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore.Metadata.Internal;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class PirateSignupPage : BaseLayout
{
    public PirateSignupPage(Database database) : base(database)
    {}
    
    public async Task<IActionResult> OnGet()
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.RedirectToPage("/login");
        
        return this.Page();
    }

    public async Task<IActionResult> OnPost()
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");

        user.IsAPirate = !user.IsAPirate;
        await this.Database.SaveChangesAsync();

        return this.Redirect("/");
    }
}