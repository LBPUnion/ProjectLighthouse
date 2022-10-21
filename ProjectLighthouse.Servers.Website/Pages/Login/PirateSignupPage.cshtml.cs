using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class PirateSignupPage : BaseLayout
{
    public PirateSignupPage(Database database) : base(database)
    {}
    
    public IActionResult OnGet()
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");
        
        return this.Page();
    }

    public async Task<IActionResult> OnPost()
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");

        user.Language = user.Language == "en-PT" ? "en" : "en-PT";
        await this.Database.SaveChangesAsync();

        return this.Redirect("/");
    }
}