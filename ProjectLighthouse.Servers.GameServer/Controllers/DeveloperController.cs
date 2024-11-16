using LBPUnion.ProjectLighthouse.Servers.GameServer.Types;
using LBPUnion.ProjectLighthouse.Types.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

public class DeveloperController : GameController
{
    [HttpGet("developer_videos")]
    public IActionResult DeveloperVideos() => this.Ok(new GameDeveloperVideos());
}