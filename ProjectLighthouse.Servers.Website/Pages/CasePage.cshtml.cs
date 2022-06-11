using LBPUnion.ProjectLighthouse.Administration;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages;

public class CasePage : BaseLayout
{
    public CasePage(Database database) : base(database)
    {}

    public List<ModerationCase> Cases = new();
    
    public async Task<IActionResult> OnGet(int pageNumber)
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.NotFound();
        if (!user.IsModerator) return this.NotFound();
        
        this.Cases.Add(new ModerationCase
        {
            CaseId = 1,
            CaseCreated = DateTime.Now,
            CaseExpires = new DateTime(2023, 11, 17),
            CaseCreatorId = user.UserId,
            CaseCreator = user,
            CaseDescription = "Being a dumbass",
            CaseType = CaseType.UserBan,
            AffectedId = user.UserId,
        });

        return this.Page();
    }
}