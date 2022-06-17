using LBPUnion.ProjectLighthouse.Administration;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Admin;

public class ModPanelPage : BaseLayout
{
    public ModPanelPage(Database database) : base(database)
    {}

    public List<AdminPanelStatistic> Statistics = new();
    
    public async Task<IActionResult> OnGet()
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");
        if (!user.IsModerator) return this.NotFound();
        
        this.Statistics.Add(new AdminPanelStatistic("Reports", await StatisticsHelper.ReportCount(), "/moderation/reports/0"));
        this.Statistics.Add(new AdminPanelStatistic("Cases", await StatisticsHelper.CaseCount(), "/moderation/cases/0"));

        return this.Page();
    }
}