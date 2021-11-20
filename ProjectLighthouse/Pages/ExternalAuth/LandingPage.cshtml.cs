#nullable enable
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages.ExternalAuth
{
    public class LandingPage : BaseLayout
    {
        public LandingPage(Database database) : base(database)
        {}

        public new User? User { get; set; }

        [UsedImplicitly]
        public async Task<IActionResult> OnGet()
        {
            User = await this.Database.UserFromWebRequest(this.Request);

            return this.Page();
        }
    }
}