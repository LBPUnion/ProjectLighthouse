using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Levels;
using LBPUnion.ProjectLighthouse.Types.Profiles;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LBPUnion.ProjectLighthouse.Controllers
{
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/xml")]
    public class PublishController : ControllerBase
    {
        private readonly Database database;

        public PublishController(Database database)
        {
            this.database = database;
        }

        /// <summary>
        ///     Endpoint the game uses to check what resources need to be uploaded and if the level can be uploaded
        /// </summary>
        [HttpPost("startPublish")]
        public async Task<IActionResult> StartPublish()
        {
            User user = await this.database.UserFromRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            Slot slot = await this.GetSlotFromBody();
            if (slot == null) return this.BadRequest(); // if the level cant be parsed then it obviously cant be uploaded

            // Republish logic
            if (slot.SlotId != 0)
            {
                Slot oldSlot = await this.database.Slots.FirstOrDefaultAsync(s => s.SlotId == slot.SlotId);
                if (oldSlot == null) return this.NotFound();
                if (oldSlot.CreatorId != user.UserId) return this.BadRequest();
            }

            string resources = slot.Resources.Where
                    (hash => !FileHelper.ResourceExists(hash))
                .Aggregate("", (current, hash) => current + LbpSerializer.StringElement("resource", hash));

            return this.Ok(LbpSerializer.TaggedStringElement("slot", resources, "type", "user"));
        }

        /// <summary>
        ///     Endpoint actually used to publish a level
        /// </summary>
        [HttpPost("publish")]
        public async Task<IActionResult> Publish()
        {
            User user = await this.database.UserFromRequest(this.Request);
            if (user == null) return this.StatusCode(403, "");

            Slot slot = await this.GetSlotFromBody();

            // Republish logic
            if (slot.SlotId != 0)
            {
                Slot oldSlot = await this.database.Slots.Include(s => s.Location).FirstOrDefaultAsync(s => s.SlotId == slot.SlotId);
                if (oldSlot == null) return this.NotFound();
                if (oldSlot.CreatorId != user.UserId) return this.BadRequest();

                oldSlot.Location.X = slot.Location.X;
                oldSlot.Location.Y = slot.Location.Y;

                slot.CreatorId = oldSlot.CreatorId;
                slot.LocationId = oldSlot.LocationId;
                slot.SlotId = oldSlot.SlotId;
                slot.FirstUploaded = oldSlot.FirstUploaded;
                slot.LastUpdated = TimeHelper.UnixTimeMilliseconds();

                this.database.Entry(oldSlot).CurrentValues.SetValues(slot);
                await this.database.SaveChangesAsync();
                return this.Ok(oldSlot.Serialize());
            }

            //TODO: parse location in body
            Location l = new()
            {
                X = slot.Location.X,
                Y = slot.Location.Y,
            };
            this.database.Locations.Add(l);
            await this.database.SaveChangesAsync();
            slot.LocationId = l.Id;
            slot.CreatorId = user.UserId;
            slot.FirstUploaded = TimeHelper.UnixTimeMilliseconds();
            slot.LastUpdated = TimeHelper.UnixTimeMilliseconds();

            this.database.Slots.Add(slot);
            await this.database.SaveChangesAsync();

            return this.Ok(slot.Serialize());
        }

        [HttpPost("unpublish/{id:int}")]
        public async Task<IActionResult> Unpublish(int id)
        {
            Slot slot = await this.database.Slots.Include(s => s.Location).FirstOrDefaultAsync(s => s.SlotId == id);

            this.database.Locations.Remove(slot.Location);
            this.database.Slots.Remove(slot);

            await this.database.SaveChangesAsync();

            return this.Ok();
        }

        public async Task<Slot> GetSlotFromBody()
        {
            this.Request.Body.Position = 0;
            string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

            XmlSerializer serializer = new(typeof(Slot));
            Slot slot = (Slot)serializer.Deserialize(new StringReader(bodyString));

            return slot;
        }
    }
}