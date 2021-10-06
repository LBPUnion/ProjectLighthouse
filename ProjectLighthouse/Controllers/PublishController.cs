using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using ProjectLighthouse.Types;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/plain")]
    public class PublishController : ControllerBase {
        [HttpPost("startPublish")]
        public async Task<IActionResult> StartPublish() {
            Request.Body.Position = 0;
            string bodyString = await new StreamReader(Request.Body).ReadToEndAsync();
            
            XmlSerializer serializer = new(typeof(Slot));
            Slot slot = (Slot)serializer.Deserialize(new StringReader(bodyString));
            
            Console.WriteLine(slot);

            return this.Ok();
        }
    }
}