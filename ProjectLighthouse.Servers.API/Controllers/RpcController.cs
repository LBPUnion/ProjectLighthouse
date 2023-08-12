using LBPUnion.ProjectLighthouse.Configuration;
using LBPUnion.ProjectLighthouse.Servers.API.Responses;
using Microsoft.AspNetCore.Mvc;

namespace LBPUnion.ProjectLighthouse.Servers.API.Controllers;

public class RpcController : ApiEndpointController
{
    /// <summary>
    /// Returns basic information that Discord RPC clients can use for self-configuration.
    /// </summary>
    /// <returns>RpcResponse</returns>
    /// <response code="200">The RPC configuration.</response>
    [HttpGet("rpc")]
    [ProducesResponseType(typeof(RpcResponse), StatusCodes.Status200OK)]
    public IActionResult GetRpcConfiguration() =>
        this.Ok(RpcResponse.CreateFromConfiguration(ServerConfiguration.Instance.RichPresenceConfiguration));
}