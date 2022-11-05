#nullable enable
using System.Buffers;
using System.IO.Pipelines;
using LBPUnion.ProjectLighthouse.Extensions;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
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
    public IActionResult ShowModerated() => this.Ok(LbpSerializer.BlankElement("resources"));

    [HttpPost("filterResources")]
    [HttpPost("showNotUploaded")]
    public async Task<IActionResult> FilterResources()
    {
        ResourceList? resourceList = await this.DeserializeBody<ResourceList>();
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
        LbpFile file = new(await readFromPipeReader(this.Request.BodyReader));

        if (!FileHelper.IsFileSafe(file))
        {
            Logger.Warn($"File is unsafe (hash: {hash}, type: {file.FileType})", LogArea.Resources);
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

    // Written with reference from
    // https://docs.microsoft.com/en-us/aspnet/core/fundamentals/middleware/request-response?view=aspnetcore-5.0
    // Surprisingly doesn't take seconds. (67ms for a 100kb file)
    private static async Task<byte[]> readFromPipeReader(PipeReader reader)
    {
        List<byte> data = new();
        while (true)
        {
            ReadResult readResult = await reader.ReadAsync();
            ReadOnlySequence<byte> buffer = readResult.Buffer;

            if (readResult.IsCompleted && buffer.Length > 0) data.AddRange(buffer.ToArray());

            reader.AdvanceTo(buffer.Start, buffer.End);

            if (readResult.IsCompleted) break;
        }

        return data.ToArray();
    }
}