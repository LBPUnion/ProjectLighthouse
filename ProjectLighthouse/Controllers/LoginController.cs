#nullable enable
using System;
using System.IO;
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
        [HttpPost]
        public async Task<IActionResult> Login() {
            if(!this.Request.Query.TryGetValue("titleID", out StringValues _))
                return this.BadRequest("");

            string body = await new StreamReader(Request.Body).ReadToEndAsync();

            LoginData loginData;
            try {
                loginData = LoginData.CreateFromString(body);
            }
            catch {
                return this.BadRequest();
            }

            await using Database database = new();

            Token? token = await database.AuthenticateUser(loginData);

            if(token == null) return this.StatusCode(403, "");

            return this.Ok(new LoginResult {
                AuthTicket = "MM_AUTH=" + token.UserToken,
                LbpEnvVer = ServerSettings.ServerName
            }.Serialize());
        }
    }
}