using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Login;

public class PirateSignupPage : BaseLayout
{
    public PirateSignupPage(DatabaseContext database) : base(database)
    {}
    
    public IActionResult OnGet()
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");
        
        return this.Page();
    }

    public async Task<IActionResult> OnPost()
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("/login");

        user.Language = user.Language == "en-PT" ? "en" : "en-PT";
        await this.Database.SaveChangesAsync();

        return this.Redirect("/");
    }
}