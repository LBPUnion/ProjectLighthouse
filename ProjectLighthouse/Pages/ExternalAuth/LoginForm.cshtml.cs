#nullable enable
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages.ExternalAuth
{
    public class LoginForm : BaseLayout
    {
        private readonly Database database;

        public LoginForm(Database database)
        {
            this.database = database;
        }

        public bool WasLoginRequest { get; private set; }

        [UsedImplicitly]
        public async Task<IActionResult> OnGet([FromQuery] string username, [FromQuery] string password)
        {
            WasLoginRequest = !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);

            if (WasLoginRequest)
            {
                User? user = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null) return this.StatusCode(403, "");

                if (!BCrypt.Net.BCrypt.Verify(password, user.Password)) return this.StatusCode(403, "");

                WebToken webToken = new()
                {
                    UserId = user.UserId,
                    UserToken = HashHelper.GenerateAuthToken(),
                };

                this.database.WebTokens.Add(webToken);
                await this.database.SaveChangesAsync();

                this.Response.Cookies.Append("LighthouseToken", webToken.UserToken);

                return this.RedirectToPage(nameof(LandingPage));
            }

            return this.Page();
        }
    }
}