using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using ProjectLighthouse.Serialization;
using ProjectLighthouse.Types;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/xml")]
    public class PublishController : ControllerBase {
        [HttpPost("startPublish")]
        public async Task<IActionResult> StartPublish() {
            Slot slot = await this.GetSlotFromBody();

            return this.Ok(LbpSerializer.TaggedStringElement("slot", "", "type", "user"));
        }

        [HttpPost("publish")]
        public async Task<IActionResult> Publish() {
            string bodyString = await new StreamReader(Request.Body).ReadToEndAsync();
            return this.Ok(LbpSerializer.TaggedStringElement("slot", bodyString, "type", "user"));
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