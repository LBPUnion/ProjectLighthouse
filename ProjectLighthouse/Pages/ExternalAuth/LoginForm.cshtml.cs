#nullable enable
using System;
using System.Threading.Tasks;
using JetBrains.Annotations;
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

                Console.WriteLine(user.UserId);
            }

            return this.Page();
        }
    }
}