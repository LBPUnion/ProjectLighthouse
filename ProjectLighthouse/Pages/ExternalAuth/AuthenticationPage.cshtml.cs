using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages.ExternalAuth
{
    public class AuthenticationPage : BaseLayout
    {

        public List<AuthenticationAttempt> AuthenticationAttempts;
        public AuthenticationPage(Database database) : base(database)
        {}

        public async Task<IActionResult> OnGet()
        {
            if (!ServerSettings.Instance.UseExternalAuth) return this.NotFound();

            this.AuthenticationAttempts = this.Database.AuthenticationAttempts.Include
                    (a => a.GameToken)
                .Where(a => a.GameToken.UserId == this.User.UserId)
                .OrderByDescending(a => a.Timestamp)
                .ToList();

            return this.Page();
        }
    }
}