using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.PlayerData.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Admin
{
    public class AdminAPIKeyPageModel : BaseLayout
    {
        public List<APIKey> APIKeys = new();
        public int KeyCount;

        public AdminAPIKeyPageModel(Database database) : base(database)
        { }

        public async Task<IActionResult> OnGet()
        {
            User? user = this.Database.UserFromWebRequest(this.Request);
            if (user == null) return this.Redirect("~/login");
            if (!user.IsAdmin) return this.NotFound();

            this.APIKeys = await this.Database.APIKeys.OrderByDescending(k => k.Id).ToListAsync();
            this.KeyCount = this.APIKeys.Count;

            return this.Page();
        }

    }
}
