using LBPUnion.ProjectLighthouse.Files;
using LBPUnion.ProjectLighthouse.Helpers;
using Microsoft.AspNetCore.Mvc;
using IOFile = System.IO.File;

namespace LBPUnion.ProjectLighthouse.Servers.Website.Controllers;

[ApiController]
public class ResourcesController : ControllerBase
{
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
        if (file != null && FileHelper.LbpFileToPNG(file))
        {
            return this.File(IOFile.OpenRead(path), "image/png");
        }
        return this.NotFound();
    }
}