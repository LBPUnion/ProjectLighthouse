using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages
{
    public class RegisterForm : BaseLayout
    {
        public RegisterForm(Database database) : base(database)
        {}

        public bool WasRegisterRequest { get; private set; }

        [UsedImplicitly]
        [SuppressMessage("ReSharper", "SpecifyStringComparison")]
        public async Task<IActionResult> OnGet([FromQuery] string username, [FromQuery] string password, [FromQuery] string confirmPassword)
        {
            this.WasRegisterRequest = !string.IsNullOrEmpty(username) && !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(confirmPassword);

            if (this.WasRegisterRequest)
            {
                if (password != confirmPassword) return this.BadRequest();

                bool userExists = await this.Database.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower()) != null;
                if (userExists) return this.BadRequest();

                User user = await this.Database.CreateUser(username, HashHelper.BCryptHash(password));

                WebToken webToken = new()
                {
                    UserId = user.UserId,
                    UserToken = HashHelper.GenerateAuthToken(),
                };

                this.Database.WebTokens.Add(webToken);
                await this.Database.SaveChangesAsync();

                this.Response.Cookies.Append("LighthouseToken", webToken.UserToken);

                return this.RedirectToPage(nameof(LandingPage));
            }

            return this.Page();
        }
    }
}