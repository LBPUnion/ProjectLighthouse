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
        private readonly Database database;
        public CommentController(Database database) {
            this.database = database;
        }

        [HttpGet("userComments/{username}")]
        public async Task<IActionResult> GetComments(string username) {
            List<Comment> comments = await database.Comments
                .Include(c => c.Target)
                .Where(c => c.Target.Username == username)
                .ToListAsync();

            string outputXml = comments.Aggregate(string.Empty, (current, comment) => current + comment.Serialize());
            return this.Ok(LbpSerializer.StringElement("comments", outputXml));
        }

        [HttpPost("postUserComment/{username}")]
        public async Task<IActionResult> PostComment(string username) {
            Request.Body.Position = 0;
            string bodyString = await new StreamReader(Request.Body).ReadToEndAsync();

            XmlSerializer serializer = new(typeof(Comment));
            Comment comment = (Comment)serializer.Deserialize(new StringReader(bodyString));

            User poster = await database.UserFromRequest(Request);

            if(poster == null) return this.StatusCode(403, "");
            
            User target = await database.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            if(comment == null || target == null) return this.BadRequest();

            comment.PosterUserId = poster.UserId;
            comment.TargetUserId = target.UserId;

            database.Comments.Add(comment);
            await database.SaveChangesAsync();
            return this.Ok();
        }
    }
}