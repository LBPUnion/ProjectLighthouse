using System.Diagnostics;
using LBPUnion.ProjectLighthouse.Administration;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Debug;

public class NewCasePage : BaseLayout
{
    public NewCasePage(Database database) : base(database)
    {}
    
    public IActionResult OnGet() => this.Page();

    public async Task<IActionResult> OnPost(
        int type, 
        string description, 
        DateTime expires,
        int affectedId
    )
    {
        ModerationCase @case = new()
        {
            CaseType = (CaseType)type,
            CaseDescription = description,
            CaseExpires = expires,
            CaseCreated = DateTime.Now,
            CaseCreatorId = 1,
            AffectedId = affectedId,
        };

        this.Database.Cases.Add(@case);
        await this.Database.SaveChangesAsync();
        
        return this.Redirect("/moderation/cases/0");
    }
}