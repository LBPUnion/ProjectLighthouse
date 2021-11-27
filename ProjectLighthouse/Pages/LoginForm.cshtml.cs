#nullable enable
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages
{
    public class LoginForm : BaseLayout
    {
        public LoginForm(Database database) : base(database)
        {}

        public bool WasLoginRequest { get; private set; }

        [UsedImplicitly]
        public async Task<IActionResult> OnGet([FromQuery] string username, [FromQuery] string password)
        {
            this.WasLoginRequest = !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password);

            if (this.WasLoginRequest)
            {
                User? user = await this.Database.Users.FirstOrDefaultAsync(u => u.Username == username);
                if (user == null) return this.StatusCode(403, "");

                if (!BCrypt.Net.BCrypt.Verify(password, user.Password)) return this.StatusCode(403, "");

                WebToken webToken = new()
                {
                    UserId = user.UserId,
                    UserToken = HashHelper.GenerateAuthToken(),
                };

                this.Database.WebTokens.Add(webToken);
                await this.Database.SaveChangesAsync();

                this.Response.Cookies.Append("LighthouseToken", webToken.UserToken);

                if (user.PasswordResetRequired) return this.Redirect("~/passwordResetRequired");

                return this.RedirectToPage(nameof(LandingPage));
            }

            return this.Page();
        }
    }
}