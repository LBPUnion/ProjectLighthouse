using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Microsoft.AspNetCore.Mvc;
using ProjectLighthouse.Helpers;
using ProjectLighthouse.Serialization;
using ProjectLighthouse.Types;
using ProjectLighthouse.Types.Files;
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
            string bodyString = await new StreamReader(Request.Body).ReadToEndAsync(); 
            
            XmlSerializer serializer = new(typeof(ResourceList));
            ResourceList resourceList = (ResourceList)serializer.Deserialize(new StringReader(bodyString));

            if(resourceList == null) return this.BadRequest();

            string resources = resourceList.Resources
                .Where(FileHelper.ResourceExists)
                .Aggregate("", (current, hash) => 
                    current + LbpSerializer.StringElement("resource", hash));

            return this.Ok(resources);
        }

        [HttpGet("r/{hash}")]
        public IActionResult GetResource(string hash) {
            string path = FileHelper.GetResourcePath(hash);

            if(FileHelper.ResourceExists(hash)) {
                return this.File(IOFile.OpenRead(path), "application/octet-stream");
            }
            return this.NotFound();
        }

        // TODO: check if this is a valid hash
        [HttpPost("upload/{hash}")]
        public async Task<IActionResult> UploadResource(string hash) {
            string assetsDirectory = FileHelper.ResourcePath;
            string path = FileHelper.GetResourcePath(hash);
            
            FileHelper.EnsureDirectoryCreated(assetsDirectory);
            if(FileHelper.ResourceExists(hash)) this.Ok(); // no reason to fail if it's already uploaded

            LbpFile file = new(Encoding.ASCII.GetBytes(await new StreamReader(Request.Body).ReadToEndAsync()));

            if(!FileHelper.IsFileSafe(file)) return this.UnprocessableEntity();
            
            await IOFile.WriteAllBytesAsync(path, file.Data);
            return this.Ok();
        }
    }
}