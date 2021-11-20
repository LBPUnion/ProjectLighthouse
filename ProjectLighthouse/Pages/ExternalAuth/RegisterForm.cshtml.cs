using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;
using JetBrains.Annotations;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Pages.ExternalAuth
{
    public class RegisterForm : BaseLayout
    {
        private readonly Database database;

        public RegisterForm(Database database)
        {
            this.database = database;
        }

        public bool WasRegisterRequest { get; private set; }

        [UsedImplicitly]
        [SuppressMessage("ReSharper", "SpecifyStringComparison")]
        public async Task<IActionResult> OnGet([FromQuery] string username, [FromQuery] string password, [FromQuery] string confirmPassword)
        {
            this.WasRegisterRequest = !string.IsNullOrEmpty(username) &&
                                      !string.IsNullOrEmpty(password) &&
                                      !string.IsNullOrEmpty(confirmPassword) &&
                                      password == confirmPassword;

            if (WasRegisterRequest)
            {
                Console.WriteLine(password);
                bool userExists = await this.database.Users.FirstOrDefaultAsync(u => u.Username.ToLower() == username.ToLower()) != null;
                if (userExists) return this.BadRequest();

                this.database.CreateUser(username, HashHelper.BCryptHash(password));
            }

            return this.Page();
        }
    }
}