#nullable enable
using LBPUnion.ProjectLighthouse.Administration.Maintenance;
using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Filter;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Servers.Website.Types;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Logging;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Admin;

public class AdminPanelPage : BaseLayout
{
    public AdminPanelPage(DatabaseContext database) : base(database)
    { }

    public List<AdminPanelStatistic> Statistics = new();

    public string? Log;

    public async Task<IActionResult> OnGet([FromQuery] string? args, [FromQuery] string? command, [FromQuery] string? maintenanceJob, [FromQuery] string? log)
    {
        UserEntity? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");
        if (!user.IsAdmin) return this.NotFound();

        this.Statistics.Add(new AdminPanelStatistic("Users", await StatisticsHelper.UserCount(this.Database), "/admin/users/0"));
        this.Statistics.Add(new AdminPanelStatistic("Slots", await StatisticsHelper.SlotCount(this.Database, new SlotQueryBuilder())));
        this.Statistics.Add(new AdminPanelStatistic("Photos", await StatisticsHelper.PhotoCount(this.Database)));
        this.Statistics.Add(new AdminPanelStatistic("API Keys", await StatisticsHelper.ApiKeyCount(this.Database), "/admin/keys"));

        if (!string.IsNullOrEmpty(command))
        {
            args ??= "";
            args = command + " " + args;
            string[] split = args.Split(" ");

            List<LogLine> runCommand = await MaintenanceHelper.RunCommand(this.HttpContext.RequestServices, split);
            return this.Redirect($"~/admin?log={CryptoHelper.ToBase64(runCommand.ToLogString())}");
        }

        if (!string.IsNullOrEmpty(maintenanceJob))
        {
            await MaintenanceHelper.RunMaintenanceJob(maintenanceJob);
            return this.Redirect("~/admin");
        }

        if (!string.IsNullOrEmpty(log))
        {
            this.Log = CryptoHelper.FromBase64(log);
        }

        return this.Page();
    }
}