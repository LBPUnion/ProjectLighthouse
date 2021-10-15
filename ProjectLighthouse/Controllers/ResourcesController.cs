using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using ProjectLighthouse.Serialization;
using IOFile = System.IO.File;

namespace ProjectLighthouse.Controllers {
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/xml")]
    public class ResourcesController : ControllerBase {
        [HttpPost("showModerated")]
        public IActionResult ShowModerated() {
            return this.Ok(LbpSerializer.BlankElement("resources"));
        }

        [HttpPost("filterResources")]
        [HttpPost("showNotUploaded")]
        public async Task<IActionResult> FilterResources() {
            return this.Ok(await new StreamReader(Request.Body).ReadToEndAsync());
        }

        [HttpGet("r/{hash}")]
        public IActionResult GetResource(string hash) {
            string path = Path.Combine(Environment.CurrentDirectory, "r", hash);

            if(IOFile.Exists(path)) {
                return this.File(IOFile.OpenRead(path), "image/jpg");
            }
            return this.NotFound();
        }

        // TODO: check if this is a valid hash
        [HttpPost("upload/{hash}")]
        public async Task<IActionResult> UploadResource(string hash) {
            string path = Path.Combine(Environment.CurrentDirectory, "r", hash);
            
            if(IOFile.Exists(path)) this.Ok(); // no reason to fail if it's already uploaded
            
            await IOFile.WriteAllTextAsync(path, await new StreamReader(Request.Body).ReadToEndAsync());
            return this.Ok();
        }
    }
}