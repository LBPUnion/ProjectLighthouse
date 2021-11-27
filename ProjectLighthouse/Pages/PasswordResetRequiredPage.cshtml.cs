#nullable enable
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages
{
    public class PasswordResetRequiredPage : BaseLayout
    {
        public PasswordResetRequiredPage([NotNull] Database database) : base(database)
        {}

        public bool WasResetRequest { get; private set; }

        public async Task<IActionResult> OnGet([FromQuery] string password, [FromQuery] string confirmPassword)
        {
            User? user = this.Database.UserFromWebRequest(this.Request);
            if (user == null) return this.Redirect("~/login");
            if (!user.PasswordResetRequired) return this.Redirect("~/resetPassword");

            this.WasResetRequest = !string.IsNullOrEmpty(password) && !string.IsNullOrEmpty(confirmPassword);

            if (this.WasResetRequest)
            {
                if (password != confirmPassword) return this.BadRequest();

                user.Password = HashHelper.BCryptHash(password);
                user.PasswordResetRequired = false;

                await this.Database.SaveChangesAsync();

                return this.Redirect("~/");
            }

            return this.Page();
        }
    }
}