using System.Collections.Generic;
using System.Threading.Tasks;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Pages.Layouts;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Pages.ExternalAuth
{
    public class AuthenticationPage : BaseLayout
    {
        public AuthenticationPage(Database database) : base(database)
        {}

        public List<AuthenticationAttempt> AuthenticationAttempts = new()
        {
            new AuthenticationAttempt
            {
                Platform = Platform.RPCS3,
                Timestamp = TimestampHelper.Timestamp,
                IPAddress = "127.0.0.1",
            },
        };

        public async Task<IActionResult> OnGet()
        {
            return this.Page();
        }
    }
}