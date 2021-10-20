using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ProjectLighthouse.Helpers;
using ProjectLighthouse.Serialization;
using ProjectLighthouse.Types;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/xml")]
    public class PublishController : ControllerBase {
        private readonly Database database;
        
        public PublishController(Database database) {
            this.database = database;
        }

        /// <summary>
        /// Endpoint the game uses to verify that the level is compatible (?)
        /// </summary>
        [HttpPost("startPublish")]
        public async Task<IActionResult> StartPublish() {
            Slot slot = await this.GetSlotFromBody();
            if(slot == null) return this.BadRequest(); // if the level cant be parsed then it obviously cant be uploaded

            string resources = slot.Resources
                .Where(hash => !FileHelper.ResourceExists(hash))
                .Aggregate("", (current, hash) => 
                    current + LbpSerializer.StringElement("resource", hash));

            return this.Ok(LbpSerializer.TaggedStringElement("slot", resources, "type", "user"));
        }

        /// <summary>
        /// Endpoint actually used to publish a level
        /// </summary>
        [HttpPost("publish")]
        public async Task<IActionResult> Publish() {
            User user = await database.UserFromRequest(Request);
            if(user == null) return this.StatusCode(403, "");
            
            Slot slot = await this.GetSlotFromBody();

            //TODO: parse location in body
            Location l = new() {
                X = 0,
                Y = 0,
            };
            database.Locations.Add(l);
            await database.SaveChangesAsync();
            slot.LocationId = l.Id;
            slot.CreatorId = user.UserId;

            database.Slots.Add(slot);
            await database.SaveChangesAsync();
            
            return this.Ok(slot.Serialize());
        }

        [HttpPost("unpublish/{id:int}")]
        public async Task<IActionResult> Unpublish(int id) {
            Slot slot = await database.Slots
                .Include(s => s.Location)
                .FirstOrDefaultAsync(s => s.SlotId == id);

            database.Locations.Remove(slot.Location);
            database.Slots.Remove(slot);

            await database.SaveChangesAsync();

            return this.Ok();
        }
        
        public async Task<Slot> GetSlotFromBody() {
            Request.Body.Position = 0;
            string bodyString = await new StreamReader(Request.Body).ReadToEndAsync();

            XmlSerializer serializer = new(typeof(Slot));
            Slot slot = (Slot)serializer.Deserialize(new StringReader(bodyString));

            return slot;
        }
    }
}