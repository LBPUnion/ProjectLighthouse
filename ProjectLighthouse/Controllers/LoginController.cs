#nullable enable
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            // FIXME: this will not do, MM_AUTH is created by the client after POST /LOGIN
            if(!this.Request.Cookies.TryGetValue("MM_AUTH", out string? mmAuth) || mmAuth == null)
                return this.BadRequest(""); // TODO: send 403

            await using Database database = new();

            Token? token;

            // ReSharper disable once InvertIf
            if(!await database.IsUserAuthenticated(mmAuth)) {
                token = await database.AuthenticateUser(mmAuth);
            }
            else {
                token = await database.Tokens.FirstOrDefaultAsync(t => t.UserToken == mmAuth);
            }

            if(token == null) return this.BadRequest(""); // TODO: send 403

            return this.Ok(new LoginResult {
                AuthTicket = token.UserToken,
                LbpEnvVer = ServerSettings.ServerName
            }.Serialize());
        }
    }
}