#nullable enable
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Helpers;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using LBPUnion.ProjectLighthouse.Types.Files;
using Microsoft.AspNetCore.Mvc;
using IOFile = System.IO.File;

namespace LBPUnion.ProjectLighthouse.Controllers.GameApi.Resources;

[ApiController]
[Produces("text/xml")]
[Route("LITTLEBIGPLANETPS3_XML")]
public class ResourcesController : ControllerBase
{
    private readonly Database database;

    public ResourcesController(Database database)
    {
        this.database = database;
    }

    [HttpPost("showModerated")]
    public IActionResult ShowModerated() => this.Ok(LbpSerializer.BlankElement("resources"));

    [HttpPost("filterResources")]
    [HttpPost("showNotUploaded")]
    public async Task<IActionResult> FilterResources()
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        string bodyString = await new StreamReader(this.Request.Body).ReadToEndAsync();

        XmlSerializer serializer = new(typeof(ResourceList));
        ResourceList? resourceList = (ResourceList?)serializer.Deserialize(new StringReader(bodyString));

        if (resourceList == null) return this.BadRequest();

        string resources = resourceList.Resources.Where
                (s => !FileHelper.ResourceExists(s))
            .Aggregate("", (current, hash) => current + LbpSerializer.StringElement("resource", hash));

        return this.Ok(LbpSerializer.StringElement("resources", resources));
    }

    [HttpGet("r/{hash}")]
    public async Task<IActionResult> GetResource(string hash)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        string path = FileHelper.GetResourcePath(hash);

        if (FileHelper.ResourceExists(hash)) return this.File(IOFile.OpenRead(path), "application/octet-stream");

        return this.NotFound();
    }

    [ResponseCache(Duration = 86400)]
    [HttpGet("/gameAssets/{hash}")]
    public IActionResult GetGameImage(string hash)
    {
        string path = Path.Combine("png", $"{hash}.png");

        if (IOFile.Exists(path))
        {
            return this.File(IOFile.OpenRead(path), "image/png");
        }

        LbpFile? file = LbpFile.FromHash(hash);
        if (file != null && ImageHelper.LbpFileToPNG(file))
        {
            return this.File(IOFile.OpenRead(path), "image/png");
        }
        return this.NotFound();
    }

    // TODO: check if this is a valid hash
    [HttpPost("upload/{hash}/unattributed")]
    [HttpPost("upload/{hash}")]
    public async Task<IActionResult> UploadResource(string hash)
    {
        User? user = await this.database.UserFromGameRequest(this.Request);
        if (user == null) return this.StatusCode(403, "");

        string assetsDirectory = FileHelper.ResourcePath;
        string path = FileHelper.GetResourcePath(hash);

        FileHelper.EnsureDirectoryCreated(assetsDirectory);
        // lbp treats code 409 as success and as an indicator that the file is already present
        if (FileHelper.ResourceExists(hash)) this.Conflict();

        Logger.LogInfo($"Processing resource upload (hash: {hash})", LogArea.Resources);
        LbpFile file = new(await BinaryHelper.ReadFromPipeReader(this.Request.BodyReader));

        if (!FileHelper.IsFileSafe(file))
        {
            Logger.LogWarn($"File is unsafe (hash: {hash}, type: {file.FileType})", LogArea.Resources);
            return this.Conflict();
        }

        string calculatedHash = file.Hash;
        if (calculatedHash != hash)
        {
            Logger.LogWarn
                ($"File hash does not match the uploaded file! (hash: {hash}, calculatedHash: {calculatedHash}, type: {file.FileType})", LogArea.Resources);
            return this.Conflict();
        }

        Logger.LogSuccess($"File is OK! (hash: {hash}, type: {file.FileType})", LogArea.Resources);
        await IOFile.WriteAllBytesAsync(path, file.Data);
        return this.Ok();
    }
}