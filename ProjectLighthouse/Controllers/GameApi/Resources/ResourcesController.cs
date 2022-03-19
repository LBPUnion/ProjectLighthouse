#nullable enable
using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Serialization;
using Kettu;
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
        if (file != null)
        {
            if (ImageHelper.LbpFileToPNG(file))
            {
                return this.File(IOFile.OpenRead(path), "image/png");
            }
        }

        return this.NotFound();
    }

    // TODO: check if this is a valid hash
    [HttpPost("upload/{hash}")]
    public async Task<IActionResult> UploadResource(string hash)
    {
        string assetsDirectory = FileHelper.ResourcePath;
        string path = FileHelper.GetResourcePath(hash);

        FileHelper.EnsureDirectoryCreated(assetsDirectory);
        // lbp treats code 409 as success and as an indicator that the file is already present
        if (FileHelper.ResourceExists(hash)) this.Conflict();

        Logger.Log($"Processing resource upload (hash: {hash})", LoggerLevelResources.Instance);
        LbpFile file = new(await BinaryHelper.ReadFromPipeReader(this.Request.BodyReader));

        if (!FileHelper.IsFileSafe(file))
        {
            Logger.Log($"File is unsafe (hash: {hash}, type: {file.FileType})", LoggerLevelResources.Instance);
            return this.Conflict();
        }

        string calculatedHash = file.Hash;
        if (calculatedHash != hash)
        {
            Logger.Log
            (
                $"File hash does not match the uploaded file! (hash: {hash}, calculatedHash: {calculatedHash}, type: {file.FileType})",
                LoggerLevelResources.Instance
            );
            return this.Conflict();
        }

        Logger.Log($"File is OK! (hash: {hash}, type: {file.FileType})", LoggerLevelResources.Instance);
        await IOFile.WriteAllBytesAsync(path, file.Data);
        return this.Ok();
    }
}