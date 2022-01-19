#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Maintenance;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages.Admin;

public class AdminPanelPage : BaseLayout
{
    public List<ICommand> Commands = MaintenanceHelper.Commands;
    public AdminPanelPage(Database database) : base(database)
    {}

    public async Task<IActionResult> OnGet([FromQuery] string? args, [FromQuery] string? command, [FromQuery] string? maintenanceJob)
    {
        User? user = this.Database.UserFromWebRequest(this.Request);
        if (user == null) return this.Redirect("~/login");
        if (!user.IsAdmin) return this.NotFound();

        if (!string.IsNullOrEmpty(command))
        {
            args ??= "";
            args = command + " " + args;
            string[] split = args.Split(" ");
            await MaintenanceHelper.RunCommand(split);
            return this.Redirect("~/admin");
        }

        if (!string.IsNullOrEmpty(maintenanceJob))
        {
            await MaintenanceHelper.RunMaintenanceJob(maintenanceJob);
            return this.Redirect("~/admin");
        }

        return this.Page();
    }
}