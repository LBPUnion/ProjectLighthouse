#nullable enable
using System.Text;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Servers.GameServer.Types.Misc;
using LBPUnion.ProjectLighthouse.Types.Logging;
using LBPUnion.ProjectLighthouse.Types.Resources;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using IOFile = System.IO.File;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Resources;

[ApiController]
[Authorize]
[Produces("text/xml")]
[Route("LITTLEBIGPLANETPS3_XML")]
public class ResourcesController : ControllerBase
{

    [HttpPost("showModerated")]
    public IActionResult ShowModerated() => this.Ok(new ResourceList());

    [HttpPost("filterResources")]
    [HttpPost("showNotUploaded")]
    public async Task<IActionResult> FilterResources()
    {
        ResourceList? resourceList = await this.DeserializeBody<ResourceList>();
        if (resourceList?.Resources == null) return this.BadRequest();

        resourceList.Resources = resourceList.Resources.Where(r => !FileHelper.ResourceExists(r)).ToArray();

        return this.Ok(resourceList);
    }

    [HttpGet("r/{hash}")]
    public IActionResult GetResource(string hash)
    {
        string path = FileHelper.GetResourcePath(hash);

        string fullPath = Path.GetFullPath(path);

        // Prevent directory traversal attacks
        if (!fullPath.StartsWith(FileHelper.FullResourcePath)) return this.BadRequest();

        if (FileHelper.ResourceExists(hash)) return this.File(IOFile.OpenRead(path), "application/octet-stream");

        return this.NotFound();
    }

    // TODO: check if this is a valid hash
    [HttpPost("upload/{hash}/unattributed")]
    [HttpPost("upload/{hash}")]
    public async Task<IActionResult> UploadResource(string hash)
    {
        string assetsDirectory = FileHelper.ResourcePath;
        string path = FileHelper.GetResourcePath(hash);
        string fullPath = Path.GetFullPath(path);

        FileHelper.EnsureDirectoryCreated(assetsDirectory);
        // lbp treats code 409 as success and as an indicator that the file is already present
        if (FileHelper.ResourceExists(hash)) return this.Conflict();

        // theoretically shouldn't be possible because of hash check but handle anyways
        if (!fullPath.StartsWith(FileHelper.FullResourcePath)) return this.BadRequest();

        Logger.Info($"Processing resource upload (hash: {hash})", LogArea.Resources);
        byte[] data = await this.Request.BodyReader.ReadAllAsync();
        LbpFile file = new(data);

        if (!FileHelper.IsFileSafe(file))
        {
            Logger.Warn($"File is unsafe (hash: {hash}, type: {file.FileType})", LogArea.Resources);
            if (file.FileType == LbpFileType.Unknown)
            {
                Logger.Warn($"({hash}): File header: '{Convert.ToHexString(data[..4])}', " +
                            $"ascii='{Encoding.ASCII.GetString(data[..4])}'",
                    LogArea.Resources);
            }
            return this.Conflict();
        }

        if (!FileHelper.AreDependenciesSafe(file))
        {
            Logger.Warn($"File has unsafe dependencies (hash: {hash}, type: {file.FileType}", LogArea.Resources);
            return this.Conflict();
        }

        string calculatedHash = file.Hash;
        if (calculatedHash != hash)
        {
            Logger.Warn
                ($"File hash does not match the uploaded file! (hash: {hash}, calculatedHash: {calculatedHash}, type: {file.FileType})", LogArea.Resources);
            return this.Conflict();
        }

        Logger.Success($"File is OK! (hash: {hash}, type: {file.FileType})", LogArea.Resources);
        await IOFile.WriteAllBytesAsync(path, file.Data);
        return this.Ok();
    }
}