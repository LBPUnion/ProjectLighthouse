using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers
{
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/xml")]
    public class PhotosController : ControllerBase
    {
        private readonly Database database;

        public PhotosController(Database database)
        {
            this.database = database;
        }

        [HttpPost("uploadPhoto")]
        public async Task<IActionResult> UploadPhoto()
        {
            this.Request.Body.Position = 0;
            string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

            XmlSerializer serializer = new(typeof(Photo));
            Photo photo = (Photo)serializer.Deserialize(new StringReader(bodyString));

            if (photo == null) return this.BadRequest();

            foreach (PhotoSubject subject in photo.Subjects)
            {
                subject.User = await this.database.Users.FirstOrDefaultAsync(u => u.Username == subject.Username);

                if (subject.User == null) return this.BadRequest();

                subject.UserId = subject.User.UserId;

                this.database.PhotoSubjects.Add(subject);
            }

            await this.database.SaveChangesAsync();

            photo.PhotoSubjectCollection = photo.Subjects.Aggregate(string.Empty, (s, subject) => s + subject.PhotoSubjectId);
//            photo.Slot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == photo.SlotId);

            this.database.Photos.Add(photo);

            await this.database.SaveChangesAsync();

            return this.Ok();
        }
    }
}