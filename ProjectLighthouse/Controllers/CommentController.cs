using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectLighthouse.Serialization;
using ProjectLighthouse.Types;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/xml")]
    public class CommentController : ControllerBase {
        [HttpGet("userComments/{username}")]
        public async Task<IActionResult> GetComments(string username) {
            // the following is downright retarded, but its 12:48am and i do not give a shit
            //                                                      ↓ ok...      ↓ why does this need to be wrapped                  ↓ again???? whyyyy
            List<Comment> comments = (await new Database().Comments.ToListAsync()).Where(c => c.TargetUsername == username).ToList(); 

            string outputXml = comments.Aggregate(string.Empty, (current, comment) => current + comment.Serialize());
            return this.Ok(LbpSerializer.StringElement("comments", outputXml));
        }

        [HttpPost("postUserComment/{username}")]
        public async Task<IActionResult> PostComment(string username) {
            Request.Body.Position = 0;
            string bodyString = await new StreamReader(Request.Body).ReadToEndAsync();

            XmlSerializer serializer = new(typeof(Comment));
            Comment comment = (Comment)serializer.Deserialize(new StringReader(bodyString));

            await using Database database = new();
            User poster = await database.Users.FirstOrDefaultAsync(u => u.Username == "jvyden");
            User target = await database.Users.FirstOrDefaultAsync(u => u.Username == username);

            comment.PosterUserId = poster.UserId;
            comment.TargetUserId = target.UserId;

            database.Comments.Add(comment);
            await database.SaveChangesAsync();

            return this.Ok();
        }
    }
}