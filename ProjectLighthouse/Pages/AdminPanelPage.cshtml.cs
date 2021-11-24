#nullable enable
using System.Collections.Generic;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.CommandLine;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages
{
    public class AdminPanelPage : BaseLayout
    {
        public AdminPanelPage(Database database) : base(database)
        {}

        public List<ICommand> Commands = CommandHelper.Commands;

        public async Task<IActionResult> OnGet([FromQuery] string? args, [FromQuery] string? command)
        {
            User? user = this.Database.UserFromWebRequest(this.Request);
            if (user == null) return this.Redirect("~/login");
            if (!user.IsAdmin) return this.NotFound();

            if (!string.IsNullOrEmpty(command))
            {
                args ??= "";
                args = command + " " + args;
                string[] split = args.Split(" ");
                await CommandHelper.RunCommand(split);
                return this.Redirect("~/admin");
            }

            return this.Page();
        }
    }
}