#nullable enable
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using ProjectLighthouse.Types;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/login")]
    [Produces("text/xml")]
    public class LoginController : ControllerBase {
        [HttpGet]
        [HttpPost]
        public async Task<IActionResult> Login() {
            if(!this.Request.Query.TryGetValue("titleID", out StringValues _))
                return this.BadRequest("");

            if(!this.Request.Cookies.TryGetValue("MM_AUTH", out string? mmAuth) || mmAuth == null)
                return this.BadRequest(""); // TODO: send 403

            await using Database database = new();

            // ReSharper disable once InvertIf
            if(!await database.IsUserAuthenticated(mmAuth)) {
                if(!await database.AuthenticateUser(mmAuth)) return this.BadRequest(""); // TODO: send 403
            }

            return this.Ok(new LoginResult {
                AuthTicket = "d2c6bbec59162a1e786ed24ad95f2b73",
                LbpEnvVer = "ProjectLighthouse"
            }.Serialize());
        }
    }
}