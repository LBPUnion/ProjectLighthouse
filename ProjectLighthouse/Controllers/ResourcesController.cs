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
        public async Task<IActionResult> FilterResources() {
            return this.Ok(await new StreamReader(Request.Body).ReadToEndAsync());
        }

        [HttpGet("r/{hash}")]
        public IActionResult GetResource(string hash) {
            string path = Path.Combine(Environment.CurrentDirectory, "r", hash);
            
            Console.WriteLine($"path: {path}, exists: {IOFile.Exists(path)}");
            
            if(IOFile.Exists(path)) {
                return this.File(IOFile.OpenRead(path), "image/jpg");
            }
            return this.NotFound();
        }
    }
}