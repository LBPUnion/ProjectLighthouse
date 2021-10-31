using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Kettu;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Files;
using Microsoft.AspNetCore.Mvc;
using IOFile = System.IO.File;

namespace LBPUnion.ProjectLighthouse.Controllers
{
    [ApiController]
    [Route("LITTLEBIGPLANETPS3_XML/")]
    [Produces("text/xml")]
    public class ResourcesController : ControllerBase
    {
        [HttpPost("showModerated")]
        public IActionResult ShowModerated() => this.Ok(LbpSerializer.BlankElement("resources"));

        [HttpPost("filterResources")]
        [HttpPost("showNotUploaded")]
        public async Task<IActionResult> FilterResources()
        {
            string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

            XmlSerializer serializer = new(typeof(ResourceList));
            ResourceList resourceList = (ResourceList)serializer.Deserialize(new StringReader(bodyString));

            if (resourceList == null) return this.BadRequest();

            string resources = resourceList.Resources.Where
                    (s => !FileHelper.ResourceExists(s))
                .Aggregate("", (current, hash) => current + LbpSerializer.StringElement("resource", hash));

            return this.Ok(LbpSerializer.StringElement("resources", resources));
        }

        [HttpGet("r/{hash}")]
        public IActionResult GetResource(string hash)
        {
            string path = FileHelper.GetResourcePath(hash);

            if (FileHelper.ResourceExists(hash)) return this.File(IOFile.OpenRead(path), "application/octet-stream");

            return this.NotFound();
        }

        // TODO: check if this is a valid hash
        [HttpPost("upload/{hash}")]
        [AllowSynchronousIo]
        public async Task<IActionResult> UploadResource(string hash)
        {

            string assetsDirectory = FileHelper.ResourcePath;
            string path = FileHelper.GetResourcePath(hash);

            FileHelper.EnsureDirectoryCreated(assetsDirectory);
            if (FileHelper.ResourceExists(hash)) this.Ok(); // no reason to fail if it's already uploaded

            Logger.Log($"Processing resource upload (hash: {hash})");
            LbpFile file = new(await BinaryHelper.ReadFromPipeReader(this.Request.BodyReader));

            if (!FileHelper.IsFileSafe(file)) return this.UnprocessableEntity();

            await IOFile.WriteAllBytesAsync(path, file.Data);
            return this.Ok();
        }
    }
}