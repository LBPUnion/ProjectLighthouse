using LBPUnion.ProjectLighthouse.Entities.Profile;
using LBPUnion.ProjectLighthouse.Entities.Token;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Admin;

public class AdminApiKeyPageModel : BaseLayout
    {
        public List<ApiKey> ApiKeys = new();
        public int KeyCount;

        public AdminApiKeyPageModel(Database database) : base(database)
        { }

        public async Task<IActionResult> OnGet()
        {
            User? user = this.Database.UserFromWebRequest(this.Request);
            if (user == null) return this.Redirect("~/login");
            if (!user.IsAdmin) return this.NotFound();

            this.ApiKeys = await this.Database.APIKeys.OrderByDescending(k => k.Id).ToListAsync();
            this.KeyCount = this.ApiKeys.Count;

            return this.Page();
        }

        public async Task<IActionResult> OnPost(string keyId)
        {
            User? user = this.Database.UserFromWebRequest(this.Request);
            if (user == null || !user.IsAdmin) return this.NotFound();

            ApiKey? apiKey = await this.Database.APIKeys.FirstOrDefaultAsync(k => k.Id == int.Parse(keyId));
            if (apiKey == null) return this.NotFound();
            this.Database.APIKeys.Remove(apiKey);
            await this.Database.SaveChangesAsync();

            return this.Page();
        }

    }