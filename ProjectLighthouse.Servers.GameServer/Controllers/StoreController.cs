using LBPUnion.ProjectLighthouse.Servers.GameServer.Types;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Controllers;

public class StoreController : GameController
{
    [HttpGet("promotions")]
    public IActionResult Promotions() => this.Ok();
}