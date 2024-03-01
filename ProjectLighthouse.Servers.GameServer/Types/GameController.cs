using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.GameServer.Types;

[ApiController]
[Authorize]
[UseDigest]
[Route("LITTLEBIGPLANETPS3_XML/")]
[Produces("text/xml")]
public class GameController : ControllerBase;