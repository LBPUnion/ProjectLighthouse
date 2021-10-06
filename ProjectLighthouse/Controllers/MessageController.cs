using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectLighthouse.Types;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/plain")]
    public class MessageController : ControllerBase {
        [HttpGet("eula")]
        public async Task<IActionResult> Eula() {
            User user = await new Database().Users.FirstOrDefaultAsync(u => u.Username == "jvyden");
            
            return Ok($"You are logged in as user {user.Username} (id {user.UserId})\n{user.Serialize()}");
        }

        [HttpGet("announce")]
        public IActionResult Announce() {
            return Ok("PROJECT LIGHTHOUSE IS A GO!\nalso ezoiar was here\nnow on ASP.NET!");
        }
    }
}