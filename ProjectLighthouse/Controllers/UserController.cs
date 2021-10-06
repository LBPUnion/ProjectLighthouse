using System;
using System.Linq;
using System.Threading.Tasks;
using System.Xml;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectLighthouse.Types;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/xml")]
    public class UserController : ControllerBase {
        [HttpGet("user/{username}")]
        public async Task<IActionResult> GetUser(string username) {
            User user = await new Database().Users.FirstOrDefaultAsync(u => u.Username == username);

            if(user == null) return this.NotFound();
            return this.Ok(user.Serialize());
        }

        [HttpPost("updateUser")]
        public async Task<IActionResult> UpdateUser() {
            await using Database database = new();
            User user = await database.Users.Where(u => u.Username == "jvyden").FirstOrDefaultAsync();

            if(user == null) return this.BadRequest();

            XmlReaderSettings settings = new() {
                Async = true
            };

            using(XmlReader reader = XmlReader.Create(Request.Body, settings)) {
                string currentElement = "";
                while(await reader.ReadAsync()) {
                    switch(reader.NodeType) {
                        case XmlNodeType.Element:
                            currentElement = reader.Name;
                            break;
                        case XmlNodeType.Text:
                            switch(currentElement) {
                                case "biography": {
                                    user.Biography = await reader.GetValueAsync();
                                    break;
                                }
                            }
                            break;
                        case XmlNodeType.EndElement:
                            currentElement = "";
                            break;
                    }
                }
            }

            await database.SaveChangesAsync();
            return this.Ok();
        }
    }
}