#nullable enable
using System.Buffers;
using System.IO.Pipelines;
using System.Xml.Serialization;
using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Logging;
using LBPUnion.ProjectLighthouse.PlayerData;
using LBPUnion.ProjectLighthouse.Serialization;
using LBPUnion.ProjectLighthouse.Types;
using Microsoft.AspNetCore.Mvc;
using IOFile = System.IO.File;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers.Resources;

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
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

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
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        string path = FileHelper.GetResourcePath(hash);

        string fullPath = Path.GetFullPath(path);
        string basePath = Path.GetFullPath(FileHelper.ResourcePath);

        // Prevent directory traversal attacks
        if (!fullPath.StartsWith(basePath)) return this.BadRequest();

        if (FileHelper.ResourceExists(hash)) return this.File(IOFile.OpenRead(path), "application/octet-stream");

        return this.NotFound();
    }

    // TODO: check if this is a valid hash
    [HttpPost("upload/{hash}/unattributed")]
    [HttpPost("upload/{hash}")]
    public async Task<IActionResult> UploadResource(string hash)
    {
        GameToken? token = await this.database.GameTokenFromRequest(this.Request);
        if (token == null) return this.StatusCode(403, "");

        string assetsDirectory = FileHelper.ResourcePath;
        string path = FileHelper.GetResourcePath(hash);

        FileHelper.EnsureDirectoryCreated(assetsDirectory);
        // lbp treats code 409 as success and as an indicator that the file is already present
        if (FileHelper.ResourceExists(hash)) return this.Conflict();

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