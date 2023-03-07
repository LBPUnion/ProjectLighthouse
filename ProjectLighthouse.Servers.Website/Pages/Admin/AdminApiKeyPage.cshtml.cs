using LBPUnion.ProjectLighthouse.Database;
using LBPUnion.ProjectLighthouse.Servers.Website.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types.Entities.Profile;
using LBPUnion.ProjectLighthouse.Types.Entities.Token;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Pages.Admin;

public class AdminApiKeyPageModel : BaseLayout
    {
        public List<ApiKeyEntity> ApiKeys = new();
        public int KeyCount;

        public AdminApiKeyPageModel(DatabaseContext database) : base(database)
        { }

        public async Task<IActionResult> OnGet()
        {
            UserEntity? user = this.Database.UserFromWebRequest(this.Request);
            if (user == null) return this.Redirect("~/login");
            if (!user.IsAdmin) return this.NotFound();

            this.ApiKeys = await this.Database.APIKeys.OrderByDescending(k => k.Id).ToListAsync();
            this.KeyCount = this.ApiKeys.Count;

            return this.Page();
        }

        public async Task<IActionResult> OnPost(string keyId)
        {
            UserEntity? user = this.Database.UserFromWebRequest(this.Request);
            if (user == null || !user.IsAdmin) return this.NotFound();

            ApiKeyEntity? apiKey = await this.Database.APIKeys.FirstOrDefaultAsync(k => k.Id == int.Parse(keyId));
            if (apiKey == null) return this.NotFound();
            this.Database.APIKeys.Remove(apiKey);
            await this.Database.SaveChangesAsync();

            return this.Page();
        }

    }