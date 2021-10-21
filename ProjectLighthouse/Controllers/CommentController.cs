using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers {
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
            List<Comment> comments = await this.database.Comments
                .Include(c => c.Target)
                .Include(c => c.Poster)
                .Where(c => c.Target.Username == username)
                .ToListAsync();

            string outputXml = comments.Aggregate(string.Empty, (current, comment) => current + comment.Serialize());
            return this.Ok(LbpSerializer.StringElement("comments", outputXml));
        }

        [HttpPost("postUserComment/{username}")]
        public async Task<IActionResult> PostComment(string username) {
            this.Request.Body.Position = 0;
            string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

            XmlSerializer serializer = new(typeof(Comment));
            Comment comment = (Comment)serializer.Deserialize(new StringReader(bodyString));

            User poster = await this.database.UserFromRequest(this.Request);

            if(poster == null) return this.StatusCode(403, "");
            
            User target = await this.database.Users.FirstOrDefaultAsync(u => u.Username == username);
            
            if(comment == null || target == null) return this.BadRequest();

            comment.PosterUserId = poster.UserId;
            comment.TargetUserId = target.UserId;

            this.database.Comments.Add(comment);
            await this.database.SaveChangesAsync();
            return this.Ok();
        }
    }
}